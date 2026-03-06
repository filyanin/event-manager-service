using EventManagerService.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerService.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<EventDTO> GetAllEvents()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id:guid}")]
        public ActionResult<EventDTO> GetEventByID(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public ActionResult<EventDTO> CreateEvent([FromBody]EventDTO newEvent)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id:guid}")]
        public ActionResult UpdateEvent(Guid id, [FromBody]EventDTO changedEvent)
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
