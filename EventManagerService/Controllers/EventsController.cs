using EventManagerService.Application.DTOs;
using EventManagerService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EventManagerService.Controllers
{
    [ApiController]
    public class EventsController : ControllerBase
    {
        public IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }


        [HttpGet]
        [Route("events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        
        public async Task<ActionResult<PaginatedResult>> GetAllEvents(string? title = null, DateTime? from = null, DateTime? to = null, [Range(1,int.MaxValue)]int page = 1, [Range(10,100)]int pageSize = 10)
        {
            var list = await _eventService.GetAllEventAsync(new Domain.Filters.EventsFilters(title, from, to), page, pageSize);

            var result = new PaginatedResult();

            result.Events = list.Items.ToList();

            result.Total = list.Total;

            result.Page = page;

            result.CurrentPageSize = pageSize;

            return Ok(result);
        }

        [HttpGet]
        [Route("events/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OutputEventDTO>> GetEventByID(Guid id)
        {
                var _event = await _eventService.GetEventByIdAsync(id);
                return Ok(_event);
        }
        
        [HttpPost]
        [Route("events")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OutputEventDTO>> CreateEvent(InputEventDTO newEvent)
        {
            var _event = await _eventService.AddEventAsync(newEvent);
            return CreatedAtAction(nameof(GetEventByID), new { id = _event.Id }, _event);
        }

        [HttpPut]
        [Route("events/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateEvent(Guid id, InputEventDTO changedEvent)
        {
            await _eventService.UpdateEventAsync(id, changedEvent);
            return Ok();
        }

        [HttpDelete]
        [Route("events/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteEvent(Guid id) 
        {
            await _eventService.DeleteEventAsync(id);
            return Ok();

        }
    }
}
