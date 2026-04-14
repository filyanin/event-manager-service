using EventManagerService.Domain.Models.Booking;

namespace EventManagerService.Domain.Interfaces.BookingService
{
    public interface IBookingService
    {

        public Task<Booking> CreateBookingAsync(Guid eventId);

        public Task<Booking> GetBookingByIdAsync(Guid bookingId);
    }
}
