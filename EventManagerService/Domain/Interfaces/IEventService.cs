using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models;

namespace EventManagerService.Domain.Interfaces
{
    public interface IEventService
    {
        public IReadOnlyList<Event> GetAllEvent(out int total, EventsFilters filters, int page = 1, int pageSize = 10);
        public Event GetEventById(Guid id);
        public Event AddEvent(string title, DateTime startAt, DateTime endAt, string? description = null);
        public void UpdateEvent(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null);
        public void DeleteEvent(Guid id);
    }
}
