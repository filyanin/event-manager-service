using EventService.Application.DTOs;
using EventService.Application.Interfaces;
using EventService.Domain.Filters;
using EventService.Domain.Models;
using EventService.Application.Interfaces;

namespace EventService.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public async Task<(IList<OutputEventDTO> Items, int Total)> GetAllEventAsync(EventsFilters filters, int page, int pageSize)
        {
            var result = await _eventRepository.GetAllAsync(filters, new EventService.Domain.ValueObjects.Paginations(pageSize, page));
            var items = result.Items.Select(e => new OutputEventDTO(e)).ToList();
            return (items, result.Total);
        }

        public async Task<OutputEventDTO> AddEventAsync(InputEventDTO eventDto)
        {
            var ev = DomainEvent.Create(Guid.NewGuid(), eventDto.Title, (DateTime)eventDto.StartAt, (DateTime)eventDto.EndAt, (int)eventDto.TotalSeat, (int)eventDto.TotalSeat, eventDto.Description);

            return new OutputEventDTO(await _eventRepository.AddAsync(ev));
        }

        public async Task<OutputEventDTO> GetEventByIdAsync(Guid id)
        {
            return new OutputEventDTO(await _eventRepository.GetByIdAsync(id));
        }

        public async Task UpdateEventAsync(Guid id, InputEventDTO eventDto)
        {
            var ev = DomainEvent.Create(id, eventDto.Title, (DateTime)eventDto.StartAt, (DateTime)eventDto.EndAt, (int)eventDto.TotalSeat, (int)eventDto.TotalSeat, eventDto.Description);
            await _eventRepository.UpdateAsync(ev);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _eventRepository.ExistsAsync(id);
        }
    }
}
