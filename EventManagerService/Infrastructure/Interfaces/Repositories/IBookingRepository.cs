using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models.DomainBooking;

namespace EventManagerService.Infrastructure.Interfaces.Repositories
{
    public interface IBookingRepository
    {
        public Task<DomainBooking> CreateAsync(Guid eventId);

        public Task<DomainBooking> GetByIdAsync(Guid bookingId);

        public Task<List<DomainBooking>> GetByStateAsync(BookingStatus state);

        public Task ConfirmAsync(Guid bookingId);

        public Task RejectAsync(Guid bookingId);

        public Task<List<Guid>> GetPendingIdsAsync(CancellationToken cancellationToken);
    }
}
