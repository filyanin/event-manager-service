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
            var outputEvent = _eventService.AddEvent(newEvent.Title, (DateTime)newEvent.StartAt, (DateTime)newEvent.EndAt, newEvent.Description);
                
            return new OutputEventDTO(outputEvent);
        }

        public void DeleteEvent(Guid id)
        {
            _eventService.DeleteEvent(id);
        }

        public PaginatedResult GetAllEvent(EventsFilters filters, int page, int pageSize)
        {
            List<OutputEventDTO> resultList = new List<OutputEventDTO>();
            int total;

            foreach (var _event in _eventService.GetAllEvent(out total,filters, page, pageSize))
            {
                resultList.Add(new OutputEventDTO(_event));
            }

            return new PaginatedResult() { Events = resultList, Total = total, CurrentPageSize = resultList.Count, Page = page};
        }

        public OutputEventDTO GetEventById(Guid id)
        {
            var _event = _eventService.GetEventById(id);

            return new OutputEventDTO(_event);
        }

        public void UpdateEvent(Guid id, InputEventDTO updatedEvent)
        {
            _eventService.UpdateEvent(
                id,
                updatedEvent.Title,
                (DateTime)updatedEvent.StartAt,
                (DateTime)updatedEvent.EndAt,
                updatedEvent.Description
                );
        }
    }
}
