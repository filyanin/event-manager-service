using EventManagerService.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerService.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<OutputEventDTO>> GetAllEvents()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id:guid}")]
        public ActionResult<OutputEventDTO> GetEventByID(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public ActionResult<InputEventDTO> CreateEvent([FromForm]InputEventDTO newEvent)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id:guid}")]
        public ActionResult UpdateEvent(Guid id, [FromForm]InputEventDTO changedEvent)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id:guid}")]
        public ActionResult DeleteEvent(Guid id) 
        {
            throw new NotImplementedException();
        }
        
    }
}
