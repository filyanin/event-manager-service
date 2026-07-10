using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models;

namespace EventManagerService.Domain.Interfaces
{
    public interface IBookingService
    {
        public Task<DomainBooking> CreateBookingAsync(Guid eventId, int seatsToReserve = 1);

        public Task<DomainBooking> GetBookingByIdAsync(Guid bookingId);

        public Task<List<DomainBooking>> GetBookingByStateAsync(BookingStatus state);

        public Task ConfirmBookingAsync(Guid bookingId);

        public Task RejectBookingAsync(Guid bookingId);
    }
}
