using EventManagerService.Application.DTOs;
using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models;

namespace EventManagerService.Application.Interfaces
{
    public interface IBookingService
    {
        public Task<BookingDTO> CreateBookingAsync(Guid eventId, int seatsToReserve = 1);

        public Task<BookingDTO> GetBookingByIdAsync(Guid bookingId);

        public Task<List<BookingDTO>> GetBookingByStateAsync(BookingStatus state);

        public Task ConfirmBookingAsync(Guid bookingId);

        public Task RejectBookingAsync(Guid bookingId);
    }
}
