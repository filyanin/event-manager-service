using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Booking;
using BookingService.Application.Interfaces;
using BookingService.Domain.Enum;

namespace BookingService.Infrastructure.Kafka.Handlers;

/// <summary>
/// Обрабатывает отказ в резервировании мест, пришедший от EventService по очереди
/// (топик BookingSeatsRejected), и переводит бронь в статус Rejected. Идемпотентно:
/// если бронь уже не в статусе Pending (например, уже отклонена или отменена),
/// сообщение просто пропускается.
/// </summary>
public class BookingSeatsRejectedEventHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<BookingSeatsRejectedEventHandler> _logger;

    public BookingSeatsRejectedEventHandler(IBookingRepository bookingRepository, ILogger<BookingSeatsRejectedEventHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task HandleAsync(BookingSeatsRejectedEvent @event)
    {
        _logger.LogInformation($"Processing BookingSeatsRejected event for booking {{{@event.BookingId}}}, reason: {@event.Reason}");

        var booking = await _bookingRepository.GetByIdAsync(@event.BookingId);
        if (booking == null)
        {
            _logger.LogWarning($"Booking {{{@event.BookingId}}} not found for BookingSeatsRejected message. Skipping.");
            return;
        }

        if (booking.Status != BookingStatus.Pending)
        {
            _logger.LogInformation($"Booking {{{@event.BookingId}}} is no longer Pending (status: {booking.Status}). Skipping duplicate/late rejection.");
            return;
        }

        booking.SetBookingRejected(DateTime.UtcNow);
        await _bookingRepository.ChangeBookingStateAsync(booking);

        _logger.LogInformation($"Booking {{{@event.BookingId}}} rejected due to: {@event.Reason}");
    }
}
