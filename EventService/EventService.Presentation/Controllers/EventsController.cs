using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventService.Application.DTOs;
using EventService.Application.Interfaces;
using EventService.Domain.Filters;
using System.Security.Claims;

namespace EventService.Presentation.Controllers;

/// <summary>
/// Контроллер для управления событиями
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
    }

    /// <summary>
    /// Получает все события с фильтрацией и пагинацией
    /// </summary>
    /// <param name="title">Фильтр по названию события (опционально)</param>
    /// <param name="from">Фильтр по дате начала (опционально)</param>
    /// <param name="to">Фильтр по дате окончания (опционально)</param>
    /// <param name="page">Номер страницы (по умолчанию 1)</param>
    /// <param name="pageSize">Размер страницы (по умолчанию 10)</param>
    /// <returns>Список событий и общее количество</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> GetEvents(
        [FromQuery] string? title = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var filters = new EventsFilters(title, from, to);
            var result = await _eventService.GetAllEventAsync(filters, page, pageSize);
            return Ok(new { items = result.Items, total = result.Total });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Получает событие по ID
    /// </summary>
    /// <param name="id">ID события</param>
    /// <returns>Данные события</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutputEventDTO>> GetEventById(Guid id)
    {
        try
        {
            var @event = await _eventService.GetEventByIdAsync(id);
            return Ok(@event);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Создаёт новое событие
    /// </summary>
    /// <param name="eventDto">Данные события</param>
    /// <returns>Созданное событие</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OutputEventDTO>> CreateEvent([FromBody] InputEventDTO eventDto)
    {
        if (eventDto == null)
            return BadRequest(new { message = "Event data is required" });

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid user ID in token" });

            var createdEvent = await _eventService.AddEventAsync(eventDto, userId);
            return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Обновляет событие
    /// </summary>
    /// <param name="id">ID события</param>
    /// <param name="eventDto">Новые данные события</param>
    /// <returns>Статус обновления</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] InputEventDTO eventDto)
    {
        if (eventDto == null)
            return BadRequest(new { message = "Event data is required" });

        try
        {
            await _eventService.UpdateEventAsync(id, eventDto);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удаляет событие
    /// </summary>
    /// <param name="id">ID события</param>
    /// <returns>Статус удаления</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        try
        {
            await _eventService.DeleteEventAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
