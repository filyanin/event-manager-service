using BookingService.Domain.Enum;
using BookingService.Domain.Models;

namespace BookingService.Application.Interfaces;

public interface IBookingRepository
{
    public Task<DomainBooking> CreateAsync(DomainBooking booking);

    public Task<DomainBooking> GetByIdAsync(Guid bookingId);

    public Task<List<DomainBooking>> GetByEventGuidAsync(Guid eventGuid);

    public Task<List<DomainBooking>> GetByUserGuidAsync(Guid userGuid);

    public Task<List<DomainBooking>> GetByStatusAsync(BookingStatus status);

    public Task ChangeBookingStateAsync(DomainBooking booking);

    public Task<List<Guid>> GetPendingIdsAsync(CancellationToken cancellationToken);

    public Task<int> GetActiveBookingCountAsync(Guid userGuid);

    public Task<bool> BookingBelongsToUserAsync(Guid bookingId, Guid userGuid);

    public Task<int> GetTotalSeatsBookedForEventAsync(Guid eventGuid);

    public Task DeleteAsync(Guid bookingId);
}
