using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Models.Event;
using EventManagerService.Properties;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;

namespace EventManagerService.Domain.Services.EventService
{
    public class EventService : IEventService
    {
        private List<Event> events = new List<Event>();

        public Event AddEvent(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var ev = Event.Create(title, startAt, endAt, description);
            events.Add(ev);
            return ev;
        }

        public void DeleteEvent(Guid id)
        {
            int index = events.FindIndex(e => e.Id.Equals(id));
            
            if (index == -1)
            {
                throw new KeyNotFoundException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), id));
            }


            events.RemoveAt(index);
        }

        public IReadOnlyList<Event> GetAllEvent(out int total, EventsFilters filters, int page, int pageSize)
        {
            if (page < 1)
            {
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("PageNumberException"), page));
            }

            if (pageSize < 10 || pageSize > 100)
            {
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("PageSizeException"), pageSize));
            }

            IEnumerable<Event> query = events;

            if (filters.Title != null)
            {
                query = query.Where(e => e.Title.ToLower().Contains(filters.Title.ToLower()));
            }
            if (filters.From != null)
            {
                query = query.Where(e => e.StartAt >= filters.From);
            }
            if (filters.To != null)
            {
                query = query.Where(e => e.EndAt <= filters.To);
            }

            total = query.Count();

            query = query.Skip((page - 1) * pageSize).Take(pageSize);        

            return query.ToList().AsReadOnly();
        }

        public Event GetEventById(Guid id)
        {
            int index = events.FindIndex(e => e.Id.Equals(id));

            if (index == -1)
            {
                throw new KeyNotFoundException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), id));
            }
            return events[index];
        }

        public void UpdateEvent(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            int index = events.FindIndex(e => e.Id.Equals(id));

            if (index == -1)
            {
               throw new KeyNotFoundException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), id));
            }
            events[index].UpdateEvent(title, startAt, endAt, description);

        }


    }
}
