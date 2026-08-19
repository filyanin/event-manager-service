using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.Events.Booking;
using Shared.Contracts.Configuration;
using Shared.Contracts.Topics;
using EventService.Application.Interfaces;

namespace EventService.Infrastructure.Kafka.Handlers;

/// <summary>
/// Обрабатывает событие создания брони (BookingCreated) и сразу же пытается
/// зарезервировать места, чтобы не допустить создания/подтверждения брони сверх
/// доступных мест. Отказ публикуется обратно в Kafka — вся координация между
/// сервисами идёт только через очередь, без прямых вызовов.
/// </summary>
public class BookingCreatedEventHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly ICacheService _cacheService;
    private readonly RedisSettings _redisSettings;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<BookingCreatedEventHandler> _logger;
    private const int MaxRetries = 3;

    public BookingCreatedEventHandler(
        IEventRepository eventRepository,
        ICacheService cacheService,
        IOptions<RedisSettings> redisSettings,
        IKafkaProducer kafkaProducer,
        ILogger<BookingCreatedEventHandler> logger)
    {
        _eventRepository = eventRepository;
        _cacheService = cacheService;
        _redisSettings = redisSettings.Value;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    public async Task HandleAsync(BookingCreatedEvent @event)
    {
        _logger.LogInformation($"Processing BookingCreated event for booking {{{@event.BookingId}}}, event {{{@event.EventGuid}}}, seats: {@event.SeatsBooked}");

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var reserved = await _eventRepository.TryReserveSeatsIdempotentAsync(@event.BookingId, @event.EventGuid, @event.SeatsBooked);
                if (!reserved)
                {
                    _logger.LogInformation($"BookingCreated message for booking {{{@event.BookingId}}} was already processed earlier. Skipping duplicate.");
                    return;
                }

                _logger.LogInformation($"Event {{{@event.EventGuid}}} updated after reserving {@event.SeatsBooked} seat(s) for booking {{{@event.BookingId}}}.");

                await _cacheService.RemoveAsync(CacheKeys.Event(_redisSettings.EventKeyPrefix, @event.EventGuid));
                return;
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning($"Event {{{@event.EventGuid}}} not found for BookingCreated message (booking {{{@event.BookingId}}}). Rejecting booking.");
                await PublishRejectionAsync(@event, "EventNotFound");
                return;
            }
            catch (InvalidOperationException ex) when (ex.Message == "NotEnoughAvailableSeats")
            {
                _logger.LogWarning($"Event {{{@event.EventGuid}}} does not have enough available seats for booking {{{@event.BookingId}}}, requested: {@event.SeatsBooked}. Rejecting booking.");
                await PublishRejectionAsync(@event, "NotEnoughAvailableSeats");
                return;
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning($"Concurrency conflict while reserving seats for event {{{@event.EventGuid}}}, attempt {attempt}/{MaxRetries}");
            }
        }

        _logger.LogError($"Failed to reserve seats for event {{{@event.EventGuid}}} after {MaxRetries} attempts due to concurrency conflicts");
        throw new InvalidOperationException($"Failed to reserve seats for event {@event.EventGuid} after {MaxRetries} attempts");
    }

    private async Task PublishRejectionAsync(BookingCreatedEvent @event, string reason)
    {
        var rejectedEvent = new BookingSeatsRejectedEvent(@event.BookingId, @event.EventGuid, reason);
        await _kafkaProducer.PublishAsync(KafkaTopics.BookingSeatsRejected, @event.BookingId.ToString(), rejectedEvent);
    }
}
