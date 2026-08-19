using EventService.Application.DTOs;
using EventService.Domain.Filters;

namespace EventService.Application.Interfaces
{
    public interface IEventService
    {
        public Task<(IList<OutputEventDTO> Items, int Total)> GetAllEventAsync(EventsFilters filters, int page, int pageSize);

        public Task<OutputEventDTO> GetEventByIdAsync(Guid id);

        /// <summary>
        /// Возвращает топ-10 событий с наибольшим процентом проданных мест.
        /// </summary>
        public Task<IList<OutputEventDTO>> GetTopEventsAsync();

        public Task<OutputEventDTO> AddEventAsync(InputEventDTO eventDto, Guid userId);

        public Task UpdateEventAsync(Guid id, InputEventDTO eventDTO);

        public Task<bool> ExistsAsync(Guid id);

        public Task DeleteEventAsync(Guid id);
    }
}

