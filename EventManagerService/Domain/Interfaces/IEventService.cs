using EventManagerService.Domain.Models;

namespace EventManagerService.Domain.Interfaces
{
    public interface IEventService
    {
        public List<Event> GetAllEvent();
        public Event GetEventById(Guid id);
        public Event AddEvent(Event newEvent);
        public bool UpdateEvent(Event updatedEvent);
        public bool DeleteEvent(Guid id);
    }
}
