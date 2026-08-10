using BookingService.Domain.Enum;
using BookingService.Domain.Models;

namespace BookingService.Application.Interfaces
{
    public interface IBookingRepository
    {
        public Task<DomainBooking> CreateAsync(DomainBooking booking);

        public Task<DomainBooking> GetByIdAsync(Guid bookingId);

        public Task<List<DomainBooking>> GetByStateAsync(BookingStatus state);

        public Task ChangeBookingStateAsync(DomainBooking booking);

        public Task<List<Guid>> GetPendingIdsAsync(CancellationToken cancellationToken);

        public Task<int> GetActiveBookingCountAsync(Guid userId);

        public Task<bool> BookingBelongsToUserAsync(Guid bookingId, Guid userId);
    }
}
