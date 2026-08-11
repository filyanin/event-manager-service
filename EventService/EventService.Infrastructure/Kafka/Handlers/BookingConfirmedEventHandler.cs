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
            DomainEvent domainEvent;
            try
            {
                domainEvent = await _eventRepository.GetByIdAsync(@event.EventGuid);
            }
            catch (KeyNotFoundException)
            {
                // Событие не найдено (например, было удалено) — пропускаем сообщение с логированием,
                // чтобы не «ронять» подписчик на одном плохом сообщении.
                _logger.LogWarning($"Event {{{@event.EventGuid}}} not found for BookingConfirmed message (booking {{{@event.BookingId}}}). Skipping message.");
                return;
            }

            if (domainEvent.AvailableSeats < @event.SeatsBooked)
            {
                _logger.LogWarning($"Event {{{@event.EventGuid}}} does not have enough available seats. Available: {domainEvent.AvailableSeats}, requested: {@event.SeatsBooked}. Skipping message.");
                return;
            }

            domainEvent.TryReserveSeats(@event.SeatsBooked);

            try
            {
                await _eventRepository.TryReserveSeatsAsync(domainEvent);
                _logger.LogInformation($"Event {{{@event.EventGuid}}} updated. Available seats: {domainEvent.AvailableSeats}");
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
