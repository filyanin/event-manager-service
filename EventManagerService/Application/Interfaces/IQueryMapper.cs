using EventManagerService.Domain.Models;
using EventManagerService.Presentation.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Application.Interfaces
{
    public interface IQueryMapper
    {
        public List<OutputEventDTO> GetAllEvent();
        public OutputEventDTO? GetEventById(Guid id);
        public OutputEventDTO AddEvent(InputEventDTO newEvent);
        public bool UpdateEvent(Guid id, InputEventDTO updatedEvent);
        public bool DeleteEvent(Guid id);
    }
}
