using EventService.Application.DTOs;
using EventService.Domain.Filters;
using EventService.Domain.Enum;

namespace EventService.Application.Interfaces
{
    public interface IEventService
    {
        public Task<(IList<OutputEventDTO> Items, int Total)> GetAllEventAsync(EventsFilters filters, int page, int pageSize);

        public Task<OutputEventDTO> GetEventByIdAsync(Guid id);

        public Task<OutputEventDTO> AddEventAsync(InputEventDTO eventDto);

        public Task UpdateEventAsync(Guid id, InputEventDTO eventDTO);

        public Task<bool> ExistsAsync(Guid id);
    }
}
