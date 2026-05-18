using EventManagerService.Domain.Models.Booking;
using EventManagerService.Presentation.DTOs.BookingService;

namespace EventManagerService.Application.Interfaces.BookingService
{
    public interface IBookingQueryMapper
    {
        public Task<BookingDTO> CreateBookingAsync(Guid eventId);

        public Task<BookingDTO> GetBookingByIdAsync(Guid bookingId);
    }
}
