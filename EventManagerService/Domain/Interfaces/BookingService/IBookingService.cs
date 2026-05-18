using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models.Booking;

namespace EventManagerService.Domain.Interfaces.BookingService
{
    public interface IBookingService
    {

        public Task<Booking> CreateBookingAsync(Guid eventId);

        public Task<Booking> GetBookingByIdAsync(Guid bookingId);

        public Task<List<Booking>> GetBookingByStateAsync(BookingStatus state);

        public Task ConfirmBookingAsync(Guid bookingId);

        public Task RejectBookingAsync(Guid bookingId);
    }
}
