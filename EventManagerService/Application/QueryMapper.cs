using EventManagerService.Application.Interfaces;
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

        public List<OutputEventDTO> GetAllEvent()
        {
            List<OutputEventDTO> resultList = new List<OutputEventDTO>();
            
            foreach (var _event in _eventService.GetAllEvent())
            {
                resultList.Add(new OutputEventDTO(_event));
            }

            return resultList;
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
