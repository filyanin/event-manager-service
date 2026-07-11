using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models;
using EventManagerService.Domain.ValueObjects;

namespace EventManagerService.Domain.Interfaces
{
    public interface IEventService
    {
        public Task<(IReadOnlyList<DomainEvent> Items, int Total)> GetAllEventAsync(EventsFilters filters, Paginations paginations);
        public Task<DomainEvent> GetEventByIdAsync(Guid id);
        public Task<DomainEvent> AddEventAsync(string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null);
        public Task UpdateEventAsync(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null);
        public Task DeleteEventAsync(Guid id);
        public Task<bool> CheckEventByIdAsync(Guid id);
    }
}
