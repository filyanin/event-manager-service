using EventManagerService.Domain.Models.Booking;

namespace EventManagerService.Domain.Interfaces.BookingService
{
    public interface IBookingService
    {

        public Booking CreateBookingAsync(Guid eventId);

        public Booking GetBookingByIdAsync(Guid bookingId);
    }
}
