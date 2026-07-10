using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models.DomainBooking;

namespace EventManagerService.Domain.Interfaces.BookingService
{
    public interface IBookingService
    {

        public Task<DomainBooking> CreateBookingAsync(Guid eventId);

        public Task<DomainBooking> GetBookingByIdAsync(Guid bookingId);

        public Task<List<DomainBooking>> GetBookingByStateAsync(BookingStatus state);

        public Task ConfirmBookingAsync(Guid bookingId);

        public Task RejectBookingAsync(Guid bookingId);
    }
}
