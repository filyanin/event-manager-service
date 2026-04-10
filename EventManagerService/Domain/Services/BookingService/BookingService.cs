using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Models.Booking;

namespace EventManagerService.Domain.Services.BookingService
{
    public class BookingService : IBookingService
    {
        private List<Booking> bookings = new List<Booking>();

        public Booking CreateBookingAsync(Guid eventId)
        {
            throw new NotImplementedException();
        }

        public Booking GetBookingByIdAsync(Guid bookingId)
        {
            throw new NotImplementedException();
        }
    }
}
