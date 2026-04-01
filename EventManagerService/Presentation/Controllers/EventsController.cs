using EventManagerService.Application.Interfaces;
using EventManagerService.Presentation.DTOs;
using EventManagerService.Properties;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Resources;

namespace EventManagerService.Presentation.Controllers
{
    [ApiController]
    public class EventsController : ControllerBase
    {
        public IQueryMapper _queryMapper;

        public EventsController(IQueryMapper queryMapper) 
        {
            _queryMapper = queryMapper;
        }


        [HttpGet]
        [Route("events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<PaginatedResult> GetAllEvents(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10)
        {
            return Ok(_queryMapper.GetAllEvent(new Domain.Filters.EventsFilters(title,from,to), page, pageSize));
        }

        [HttpGet]
        [Route("events/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<OutputEventDTO> GetEventByID(Guid id)
        {
            var _event = _queryMapper.GetEventById(id);

            if (_event == null)
                return NotFound(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), id));

            return Ok(_event);
        }
        
        [HttpPost]
        [Route("events")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult<OutputEventDTO> CreateEvent(InputEventDTO newEvent)
        {
            var _event = _queryMapper.AddEvent(newEvent);
            return CreatedAtAction(nameof(GetEventByID), new { id = _event.Id }, _event);
        }

        [HttpPut]
        [Route("events/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateEvent(Guid id, InputEventDTO changedEvent)
        {
            if (_queryMapper.UpdateEvent(id,changedEvent))
                return Ok();
            return NotFound(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), id));
        }

        [HttpDelete]
        [Route("events/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteEvent(Guid id) 
        {
            if (_queryMapper.DeleteEvent(id))
                return Ok();
            return NotFound(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), id));
        }


        [HttpGet]
        [Route("broke/{i:int}")]
        public ActionResult BrokeEvent(int i)
        {
            List<int> test = new List<int>();

            test[i].CompareTo(1);

            return Ok();

        }
    }
}
