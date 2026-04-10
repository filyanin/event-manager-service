using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models;
using EventManagerService.Presentation.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Application.Interfaces
{
    public interface IQueryMapper
    {
        public PaginatedResult GetAllEvent(EventsFilters filters, int page = 1, int pageSize = 10);
        public OutputEventDTO GetEventById(Guid id);
        public OutputEventDTO AddEvent(InputEventDTO newEvent);
        public void UpdateEvent(Guid id, InputEventDTO updatedEvent);
        public void DeleteEvent(Guid id);
    }
}
