using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models;

namespace EventManagerService.Application.Interfaces
{
    public interface IBookingRepository
    {
        public Task<DomainBooking> CreateAsync(DomainBooking booking);

        public Task<DomainBooking> GetByIdAsync(Guid bookingId);

        public Task<List<DomainBooking>> GetByStateAsync(BookingStatus state);

        public Task ChangeBookingStateAsync(DomainBooking booking);

        public Task<List<Guid>> GetPendingIdsAsync(CancellationToken cancellationToken);
    }
}
