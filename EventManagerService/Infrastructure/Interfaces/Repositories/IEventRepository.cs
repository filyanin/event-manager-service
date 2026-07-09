using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models.DomainEvent;

namespace EventManagerService.Infrastructure.Interfaces.Repositories
{
    public interface IEventRepository
    {
        public Task<(IReadOnlyList<DomainEvent> Items, int Total)> GetAllAsync(EventsFilters filters, int page, int pageSize);

        public Task<DomainEvent> AddAsync(DomainEvent ev);

        public Task DeleteAsync(Guid id);

        public Task<DomainEvent> GetByIdAsync(Guid id);

        public Task UpdateAsync(DomainEvent domainEvent);

        public Task<bool> ExistsAsync(Guid id);

        public Task<bool> TryReserveSeatsAsync(Guid id, int count = 1);

        public Task<bool> ReleaseSeatsAsync(Guid id, int count = 1);
    }
}
