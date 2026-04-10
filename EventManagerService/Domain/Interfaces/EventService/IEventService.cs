using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models.Event;

namespace EventManagerService.Domain.Interfaces.EventService
{
    public interface IEventService
    {
        public IReadOnlyList<Event> GetAllEvent(out int total, EventsFilters filters, int page, int pageSize);
        public Event GetEventById(Guid id);
        public Event AddEvent(string title, DateTime startAt, DateTime endAt, string? description = null);
        public void UpdateEvent(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null);
        public void DeleteEvent(Guid id);
    }
}
