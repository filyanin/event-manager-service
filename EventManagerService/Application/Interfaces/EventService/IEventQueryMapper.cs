using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models;
using EventManagerService.Presentation.DTOs.EventService;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Application.Interfaces.EventService
{
    public interface IEventQueryMapper
    {
        public Task<PaginatedResult> GetAllEventAsync(EventsFilters filters, int page = 1, int pageSize = 10);
        public Task<OutputEventDTO> GetEventByIdAsync(Guid id);
        public Task<OutputEventDTO> AddEventAsync(InputEventDTO newEvent);
        public Task UpdateEventAsync(Guid id, InputEventDTO updatedEvent);
        public Task DeleteEventAsync(Guid id);
    }
}
