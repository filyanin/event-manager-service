using EventManagerService.Application.Interfaces;
using EventManagerService.Presentation.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerService.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        public IQuerryMapper _querryMapper;

        public EventsController(IQuerryMapper querryMapper) 
        {
            _querryMapper = querryMapper;
        }

        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<OutputEventDTO>> GetAllEvents()
        {
            return Ok(_querryMapper.GetAllEvent());
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<OutputEventDTO> GetEventByID(Guid id)
        {
            var _event = _querryMapper.GetEventById(id);

            if (_event == null)
                return NotFound($"Oblect with id {id} has not found");

            return Ok(_event);
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult<OutputEventDTO> CreateEvent([FromForm]InputEventDTO newEvent)
        {
            var _event = _querryMapper.AddEvent(newEvent);
            return CreatedAtAction(nameof(GetEventByID), new { id = _event.Id }, _event);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateEvent(Guid id, [FromForm]InputEventDTO changedEvent)
        {
            if (_querryMapper.UpdateEvent(id,changedEvent))
                return Ok();
            return NotFound();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteEvent(Guid id) 
        {
            if (_querryMapper.DeleteEvent(id))
                return Ok();
            return NotFound();
        }
        
    }
}
