using EventManagerService.Application.Interfaces;
using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Interfaces;
using EventManagerService.Presentation.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Application
{
    public class QueryMapper : IQueryMapper
    {
        private IEventService _eventService;

        public QueryMapper(IEventService eventService)
        {
            _eventService = eventService;
        }

        public OutputEventDTO AddEvent(InputEventDTO newEvent)
        {
            var outputEvent = _eventService.AddEvent(
                Domain.Models.Event.Create(newEvent.Title, (DateTime)newEvent.StartAt, (DateTime)newEvent.EndAt, newEvent.Description));

            return new OutputEventDTO(outputEvent);
        }

        public bool DeleteEvent(Guid id)
        {
            return _eventService.DeleteEvent(id);
        }

        public PaginatedResult GetAllEvent(EventsFilters filters, int page = 1, int pageSize = 10)
        {
            List<OutputEventDTO> resultList = new List<OutputEventDTO>();
            int total;

            foreach (var _event in _eventService.GetAllEvent(out total,filters, page, pageSize))
            {
                resultList.Add(new OutputEventDTO(_event));
            }

            return new PaginatedResult() { Events = resultList, Total = total, CurrentPageSize = resultList.Count, Page = page};
        }

        public OutputEventDTO? GetEventById(Guid id)
        {
            var _event = _eventService.GetEventById(id);

            if (_event == null)
            {
                return null;
            }
            return new OutputEventDTO(_event);
        }

        public bool UpdateEvent(Guid id, InputEventDTO updatedEvent)
        {
            return _eventService.UpdateEvent(
                id,
                updatedEvent.Title,
                (DateTime)updatedEvent.StartAt,
                (DateTime)updatedEvent.EndAt,
                updatedEvent.Description
                );
        }
    }
}
