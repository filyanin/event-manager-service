using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Domain
{
    public class EventService : IEventService
    {
        private List<Event> events = new List<Event>();

        public Event AddEvent(Event newEvent)
        {
            events.Add(newEvent);
            return newEvent;
        }

        public bool DeleteEvent(Guid id)
        {
            int index = events.FindIndex(e => e.Id.Equals(id));
            
            if (index != -1)
            {
                events.RemoveAt(index);
                return true;
            }
            return false;
        }

        public IReadOnlyList<Event> GetAllEvent(out int total, EventsFilters filters, int page = 1, int pageSize = 10)
        {
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

        public Event? GetEventById(Guid id)
        {
            return events.Find(e => e.Id.Equals(id));
        }

        public bool UpdateEvent(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            int index = events.FindIndex(e => e.Id.Equals(id));

            if (index != -1)
            {
                events[index].UpdateEvent(title, startAt, endAt, description);
                return true;
            }
            return false;
        }


    }
}
