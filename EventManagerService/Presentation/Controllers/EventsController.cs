using EventManagerService.Application.Interfaces.EventService;
using EventManagerService.Presentation.DTOs.EventService;
using EventManagerService.Presentation.Validators;
using EventManagerService.Properties;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Resources;

namespace EventManagerService.Presentation.Controllers
{
    [ApiController]
    public class EventsController : ControllerBase
    {
        public IEventQueryMapper _queryMapper;

        public EventsController(IEventQueryMapper queryMapper) 
        {
            _queryMapper = queryMapper;
        }


        [HttpGet]
        [Route("events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        
        public async Task<ActionResult<PaginatedResult>> GetAllEvents(string? title = null, DateTime? from = null, DateTime? to = null, [Range(1,int.MaxValue)]int page = 1, [Range(10,100)]int pageSize = 10)
        {
            return Ok(await _queryMapper.GetAllEventAsync(new Domain.Filters.EventsFilters(title,from,to), page, pageSize));
        }

        [HttpGet]
        [Route("events/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OutputEventDTO>> GetEventByID(Guid id)
        {
                var _event = await _queryMapper.GetEventByIdAsync(id);
                return Ok(_event);
        }
        
        [HttpPost]
        [Route("events")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OutputEventDTO>> CreateEvent(InputEventDTO newEvent)
        {
            var _event = await _queryMapper.AddEventAsync(newEvent);
            return CreatedAtAction(nameof(GetEventByID), new { id = _event.Id }, _event);
        }

        [HttpPut]
        [Route("events/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateEvent(Guid id, InputEventDTO changedEvent)
        {
            await _queryMapper.UpdateEventAsync(id, changedEvent);
            return Ok();
        }

        [HttpDelete]
        [Route("events/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteEvent(Guid id) 
        {
            await _queryMapper.DeleteEventAsync(id);
            return Ok();

        }
    }
}
