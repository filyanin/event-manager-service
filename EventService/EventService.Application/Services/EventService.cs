using EventService.Application.DTOs;
using EventService.Application.Interfaces;
using EventService.Domain.Filters;
using EventService.Domain.Models;
using EventService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.Configuration;

namespace EventService.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ICacheService _cacheService;
    private readonly RedisSettings _redisSettings;
    private readonly ILogger<EventService> _logger;

    private const int TopEventsCount = 10;

    public EventService(
        IEventRepository eventRepository,
        ICacheService cacheService,
        IOptions<RedisSettings> redisSettings,
        ILogger<EventService> logger)
    {
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _redisSettings = redisSettings?.Value ?? throw new ArgumentNullException(nameof(redisSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(IList<OutputEventDTO> Items, int Total)> GetAllEventAsync(EventsFilters filters, int page, int pageSize)
    {
        var paginations = new Paginations(pageSize, page);
        var result = await _eventRepository.GetAllAsync(filters, paginations);
        var items = result.Items.Select(e => new OutputEventDTO(e)).ToList();
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
        return new OutputEventDTO(createdEvent);
    }

    public async Task<OutputEventDTO> GetEventByIdAsync(Guid id)
    {
        var cacheKey = CacheKeys.Event(_redisSettings.EventKeyPrefix, id);
        var cached = await _cacheService.GetAsync<OutputEventDTO>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var ev = await _eventRepository.GetByIdAsync(id);
        var dto = new OutputEventDTO(ev);

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromSeconds(_redisSettings.EventTtlSeconds));

        return dto;
    }

    public async Task<IList<OutputEventDTO>> GetTopEventsAsync()
    {
        var cacheKey = _redisSettings.TopEventsKey;
        var cached = await _cacheService.GetAsync<IList<OutputEventDTO>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var topEvents = await _eventRepository.GetTopEventsAsync(TopEventsCount);
        var dtos = topEvents.Select(e => new OutputEventDTO(e)).ToList();

        await _cacheService.SetAsync<IList<OutputEventDTO>>(cacheKey, dtos, TimeSpan.FromSeconds(_redisSettings.TopEventsTtlSeconds));

        return dtos;
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

        // Сначала сохраняем в базу, затем инвалидируем кеш, чтобы при сбое между шагами
        // база оставалась источником истины, а кеш просто прогрелся заново при следующем чтении.
        await _cacheService.RemoveAsync(CacheKeys.Event(_redisSettings.EventKeyPrefix, id));
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

        await _cacheService.RemoveAsync(CacheKeys.Event(_redisSettings.EventKeyPrefix, id));
    }
}
