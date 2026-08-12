using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Booking;
using EventService.Application.Interfaces;
using EventService.Domain.Models;

namespace EventService.Infrastructure.Kafka.Handlers;

public class BookingConfirmedEventHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<BookingConfirmedEventHandler> _logger;
    private const int MaxRetries = 3;

    public BookingConfirmedEventHandler(IEventRepository eventRepository, ILogger<BookingConfirmedEventHandler> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task HandleAsync(BookingConfirmedEvent @event)
    {
        _logger.LogInformation($"Processing BookingConfirmed event for booking {{{@event.BookingId}}}, event {{{@event.EventGuid}}}, seats: {@event.SeatsBooked}");

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var reserved = await _eventRepository.TryReserveSeatsIdempotentAsync(@event.BookingId, @event.EventGuid, @event.SeatsBooked);
                if (!reserved)
                {
                    _logger.LogInformation($"BookingConfirmed message for booking {{{@event.BookingId}}} was already processed earlier. Skipping duplicate.");
                    return;
                }

                _logger.LogInformation($"Event {{{@event.EventGuid}}} updated after reserving {@event.SeatsBooked} seat(s).");
                return;
            }
            catch (KeyNotFoundException)
            {
                // Событие не найдено (например, было удалено) — пропускаем сообщение с логированием,
                // чтобы не «ронять» подписчик на одном плохом сообщении.
                _logger.LogWarning($"Event {{{@event.EventGuid}}} not found for BookingConfirmed message (booking {{{@event.BookingId}}}). Skipping message.");
                return;
            }
            catch (InvalidOperationException ex) when (ex.Message == "NotEnoughAvailableSeats")
            {
                _logger.LogWarning($"Event {{{@event.EventGuid}}} does not have enough available seats for booking {{{@event.BookingId}}}, requested: {@event.SeatsBooked}. Skipping message.");
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
}
