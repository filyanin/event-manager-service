using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.Events.Booking;
using Shared.Contracts.Configuration;
using EventService.Application.Interfaces;

namespace EventService.Infrastructure.Kafka.Handlers;

public class BookingCancelledEventHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly ICacheService _cacheService;
    private readonly RedisSettings _redisSettings;
    private readonly ILogger<BookingCancelledEventHandler> _logger;
    private const int MaxRetries = 3;

    public BookingCancelledEventHandler(
        IEventRepository eventRepository,
        ICacheService cacheService,
        IOptions<RedisSettings> redisSettings,
        ILogger<BookingCancelledEventHandler> logger)
    {
        _eventRepository = eventRepository;
        _cacheService = cacheService;
        _redisSettings = redisSettings.Value;
        _logger = logger;
    }

    public async Task HandleAsync(BookingCancelledEvent @event)
    {
        _logger.LogInformation($"Processing BookingCancelled event for booking {{{@event.BookingId}}}, event {{{@event.EventGuid}}}, seats: {@event.SeatsReleased}");

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var released = await _eventRepository.ReleaseSeatsIdempotentAsync(@event.BookingId, @event.EventGuid, @event.SeatsReleased);
                if (!released)
                {
                    _logger.LogInformation($"BookingCancelled message for booking {{{@event.BookingId}}} was already processed earlier. Skipping duplicate.");
                    return;
                }

                _logger.LogInformation($"Event {{{@event.EventGuid}}} updated after releasing {@event.SeatsReleased} seat(s).");

                await _cacheService.RemoveAsync(CacheKeys.Event(_redisSettings.EventKeyPrefix, @event.EventGuid));
                return;
            }
            catch (KeyNotFoundException)
            {
                // Событие не найдено (например, было удалено) — пропускаем сообщение с логированием,
                // чтобы не «ронять» подписчик на одном плохом сообщении.
                _logger.LogWarning($"Event {{{@event.EventGuid}}} not found for BookingCancelled message (booking {{{@event.BookingId}}}). Skipping message.");
                return;
            }
            catch (InvalidOperationException ex) when (ex.Message == "TooManySeatsToRelease")
            {
                _logger.LogWarning($"Event {{{@event.EventGuid}}} cannot release {@event.SeatsReleased} seat(s) without exceeding total capacity for booking {{{@event.BookingId}}}. Skipping message.");
                return;
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning($"Concurrency conflict while releasing seats for event {{{@event.EventGuid}}}, attempt {attempt}/{MaxRetries}");
            }
        }

        _logger.LogError($"Failed to release seats for event {{{@event.EventGuid}}} after {MaxRetries} attempts due to concurrency conflicts");
        throw new InvalidOperationException($"Failed to release seats for event {@event.EventGuid} after {MaxRetries} attempts");
    }
}
