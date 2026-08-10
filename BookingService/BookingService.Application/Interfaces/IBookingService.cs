using BookingService.Application.DTOs;
using BookingService.Domain.Enum;

namespace BookingService.Application.Interfaces
{
    public interface IBookingService
    {
        public Task<BookingDTO> CreateBookingAsync(Guid eventId, Guid userId, int seatsToReserve = 1);

        public Task<BookingDTO> GetBookingByIdAsync(Guid bookingId);

        public Task<List<BookingDTO>> GetBookingByStateAsync(BookingStatus state);

        public Task ConfirmBookingAsync(Guid bookingId);

        public Task RejectBookingAsync(Guid bookingId);

        public Task CancelBookingAsync(Guid bookingId, Guid userId, string userRole = "user");
    }
}
