using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models;
using EventManagerService.Domain.ValueObjects;

namespace EventManagerService.Application.Interfaces
{
    public interface IEventRepository
    {
        public Task<(IList<DomainEvent> Items, int Total)> GetAllAsync(EventsFilters filters, Paginations paginations);

        public Task<DomainEvent> AddAsync(DomainEvent ev);

        public Task DeleteAsync(Guid id);

        public Task<DomainEvent> GetByIdAsync(Guid id);

        public Task UpdateAsync(DomainEvent domainEvent);

        public Task<bool> ExistsAsync(Guid id);

        public Task<bool> TryReserveSeatsAsync(DomainEvent @event);

        public Task<bool> ReleaseSeatsAsync(DomainEvent @event);
    }
}
