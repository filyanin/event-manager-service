using EventManagerService.Application.Interfaces;
using EventManagerService.Domain.Interfaces;
using EventManagerService.Presentation.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Application
{
    public class QuerryMapper : IQuerryMapper
    {
        private IEventService _eventService;

        public QuerryMapper(IEventService eventService)
        {
            _eventService = eventService;
        }

        public OutputEventDTO AddEvent(InputEventDTO newEvent)
        {
            if (newEvent.startAt == null)
            {
                throw (new ArgumentException("start Date must be not null"));
            }

            if (newEvent.endAt == null)
            {
                throw (new ArgumentException("end Date must be not null"));
            }


            var outputEvent = _eventService.AddEvent(
                Domain.Models.Event.Create(newEvent.title, (DateTime)newEvent.startAt, (DateTime)newEvent.endAt, newEvent.description));

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
            if (updatedEvent.startAt == null)
            {
                throw (new ArgumentException("start Date must be not null"));
            }

            if (updatedEvent.endAt == null)
            {
                throw (new ArgumentException("end Date must be not null"));
            }

            return _eventService.UpdateEvent(
                id,
                updatedEvent.title,
                (DateTime)updatedEvent.startAt,
                (DateTime)updatedEvent.endAt,
                updatedEvent.description
                );
        }
    }
}
