using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Booking;
using EventService.Application.Interfaces;

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
        try
        {
            _logger.LogInformation($"Processing BookingConfirmed event for event {{{@event.EventGuid}}}, seats: {@event.SeatsBooked}");

            for (var attempt = 1; attempt <= MaxRetries; attempt++)
            {
                var domainEvent = await _eventRepository.GetByIdAsync(@event.EventGuid);

                if (domainEvent.AvailableSeats < @event.SeatsBooked)
                {
                    _logger.LogWarning($"Event {{{@event.EventGuid}}} does not have enough available seats. Available: {domainEvent.AvailableSeats}, requested: {@event.SeatsBooked}");
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
        catch (Exception ex)
        {
            _logger.LogError($"Error handling BookingConfirmed event: {ex.Message}. Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
