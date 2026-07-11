using EventManagerService.Application.DTOs;
using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models;
using EventManagerService.Domain.ValueObjects;

namespace EventManagerService.Application.Interfaces
{
    public interface IEventService
    {
        public Task<(IReadOnlyList<OutputEventDTO> Items, int Total)> GetAllEventAsync(EventsFilters filters, int page, int pageSize);
        public Task<OutputEventDTO> GetEventByIdAsync(Guid id);
        public Task<OutputEventDTO> AddEventAsync(InputEventDTO eventDto);
        public Task UpdateEventAsync(Guid id, InputEventDTO eventDTO);
        public Task DeleteEventAsync(Guid id);
        public Task<bool> CheckEventByIdAsync(Guid id);
    }
}
