using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Booking;
using EventService.Application.Interfaces;

namespace EventService.Infrastructure.Kafka.Handlers;

public class BookingConfirmedEventHandler
{
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<BookingConfirmedEventHandler> _logger;

    public BookingConfirmedEventHandler(IEventRepository eventRepository, ILogger<BookingConfirmedEventHandler> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task HandleAsync(BookingConfirmedEvent @event)
    {
        try
        {
            _logger.LogInformation($"Processing BookingConfirmed event for event {{{@event.EventGuid}}}, seats: {@event.SeatsBooked}");

            var domainEvent = await _eventRepository.GetByIdAsync(@event.EventGuid);
            if (domainEvent == null)
            {
                _logger.LogWarning($"Event {{{@event.EventGuid}}} not found, skipping seats reduction");
                return;
            }

            // Уменьшаем доступные места
            if (domainEvent.AvailableSeats >= @event.SeatsBooked)
            {
                domainEvent.ReserveSeats(@event.SeatsBooked);
                await _eventRepository.UpdateAsync(domainEvent);
                _logger.LogInformation($"Event {{{@event.EventGuid}}} updated. Available seats: {domainEvent.AvailableSeats}");
            }
            else
            {
                _logger.LogWarning($"Event {{{@event.EventGuid}}} does not have enough available seats. Available: {domainEvent.AvailableSeats}, requested: {@event.SeatsBooked}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling BookingConfirmed event: {ex.Message}. Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
