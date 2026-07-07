using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using DomainEvent = EventManagerService.Domain.Models.DomainEvent.DomainEvent;
using DataEvent = EventManagerService.Infrastructure.DataAssets.Models.Event;
using EventManagerService.Domain.Interfaces.Repositories;

namespace EventManagerService.Domain.Services.EventService
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<(IReadOnlyList<DomainEvent> Items, int Total)> GetAllEventAsync(EventsFilters filters, int page, int pageSize)
        {
            return await _eventRepository.GetAllAsync(filters, page, pageSize);
        }

        public async Task<DomainEvent> AddEventAsync(string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null)
        {
            var ev = DomainEvent.Create(title, startAt, endAt, totalSeats, description);
            return await _eventRepository.AddAsync(ev);
        }

        public void DeleteEvent(Guid id)
        {
            DeleteEventAsync(id).GetAwaiter().GetResult();
        }

        public async Task DeleteEventAsync(Guid id)
        {
            await _eventRepository.DeleteAsync(id);
        }

        public async Task<(IReadOnlyList<DomainEvent> list, int total)> GetAllEventAsync_Internal(EventsFilters filters, int page, int pageSize)
        {
            return await _eventRepository.GetAllAsync(filters, page, pageSize);
        }

        public async Task<DomainEvent> GetEventByIdAsync(Guid id)
        {
            return await _eventRepository.GetByIdAsync(id);
        }

        public async Task UpdateEventAsync(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var ev = DomainEvent.Create(title, startAt, endAt, 0, description);
            await _eventRepository.UpdateAsync(ev);
        }

        public async Task<bool> CheckEventByIdAsync(Guid id)
        {
            return await _eventRepository.ExistsAsync(id);
        }
    }
}
