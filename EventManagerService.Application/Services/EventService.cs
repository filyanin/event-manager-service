using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Models;
using EventManagerService.Domain.ValueObjects;
using EventManagerService.Infrastructure.Interfaces.Repositories;

namespace EventManagerService.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<(IReadOnlyList<DomainEvent> Items, int Total)> GetAllEventAsync(EventsFilters filters, Paginations paginations)
        {
            return await _eventRepository.GetAllAsync(filters, paginations);
        }

        public async Task<DomainEvent> AddEventAsync(string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null)
        {
            var ev = DomainEvent.Create(Guid.NewGuid(), title, startAt, endAt, totalSeats,totalSeats, description);

            return await _eventRepository.AddAsync(ev);
        }

        public async Task DeleteEventAsync(Guid id)
        {
            await _eventRepository.DeleteAsync(id);
        }

        public async Task<DomainEvent> GetEventByIdAsync(Guid id)
        {
            return await _eventRepository.GetByIdAsync(id);
        }

        public async Task UpdateEventAsync(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var ev = DomainEvent.Create(id, title, startAt, endAt, 0, 0, description);
            await _eventRepository.UpdateAsync(ev);
        }

        public async Task<bool> CheckEventByIdAsync(Guid id)
        {
            return await _eventRepository.ExistsAsync(id);
        }
    }
}
