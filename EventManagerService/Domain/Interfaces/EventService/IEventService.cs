using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models.Event;

namespace EventManagerService.Domain.Interfaces.EventService
{
    public interface IEventService
    {
        public Task<(IReadOnlyList<Event> Items, int Total)> GetAllEventAsync(EventsFilters filters, int page, int pageSize);
        public Task<Event> GetEventByIdAsync(Guid id);
        public Task<Event> AddEventAsync(string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null);
        public Task UpdateEventAsync(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null);
        public Task DeleteEventAsync(Guid id);
        public Task<bool> CheckEventByIdAsync(Guid id);
    }
}
