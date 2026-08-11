using EventService.Application.DTOs;
using EventService.Application.Interfaces;
using EventService.Domain.Filters;
using EventService.Domain.Models;
using EventService.Domain.ValueObjects;

namespace EventService.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    }

    public async Task<(IList<OutputEventDTO> Items, int Total)> GetAllEventAsync(EventsFilters filters, int page, int pageSize)
    {
        var paginations = new Paginations(pageSize, page);
        var result = await _eventRepository.GetAllAsync(filters, paginations);
        var items = result.Items.Select(e => new OutputEventDTO(e, DateTime.UtcNow)).ToList();
        return (items, result.Total);
    }

    public async Task<OutputEventDTO> AddEventAsync(InputEventDTO eventDto, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(eventDto.Title))
            throw new ArgumentException("Title is required", nameof(eventDto.Title));

        if (!eventDto.StartAt.HasValue || !eventDto.EndAt.HasValue)
            throw new ArgumentException("StartAt and EndAt are required");

        if (!eventDto.TotalSeat.HasValue || eventDto.TotalSeat <= 0)
            throw new ArgumentException("TotalSeat must be greater than 0");

        var ev = DomainEvent.Create(
            Guid.NewGuid(), 
            eventDto.Title, 
            eventDto.StartAt.Value, 
            eventDto.EndAt.Value, 
            eventDto.TotalSeat.Value, 
            userId, 
            eventDto.Description
        );

        var createdEvent = await _eventRepository.AddAsync(ev);
        return new OutputEventDTO(createdEvent, DateTime.UtcNow);
    }

    public async Task<OutputEventDTO> GetEventByIdAsync(Guid id)
    {
        var ev = await _eventRepository.GetByIdAsync(id);
        return new OutputEventDTO(ev, DateTime.UtcNow);
    }

    public async Task UpdateEventAsync(Guid id, InputEventDTO eventDto)
    {
        if (string.IsNullOrWhiteSpace(eventDto.Title))
            throw new ArgumentException("Title is required", nameof(eventDto.Title));

        if (!eventDto.StartAt.HasValue || !eventDto.EndAt.HasValue)
            throw new ArgumentException("StartAt and EndAt are required");

        var existingEvent = await _eventRepository.GetByIdAsync(id);
        if (existingEvent == null)
            throw new InvalidOperationException($"Event with id {id} not found");

        existingEvent.UpdateEvent(eventDto.Title, eventDto.StartAt.Value, eventDto.EndAt.Value, eventDto.Description);

        await _eventRepository.UpdateAsync(existingEvent);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _eventRepository.ExistsAsync(id);
    }

    public async Task DeleteEventAsync(Guid id)
    {
        if (!await _eventRepository.ExistsAsync(id))
            throw new InvalidOperationException($"Event with id {id} not found");

        await _eventRepository.DeleteAsync(id);
    }
}
