using EventManagerService.Application.DTOs;
using EventManagerService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerService.Controllers
{

    [ApiController]
    public class BookingController : ControllerBase
    {
        IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        [Route("events/{id:guid}/book")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookingDTO>> CreateBooking(Guid id)
        {
            // TODO: получить userId из контекста пользователя (claims, HttpContext)
            var userId = Guid.NewGuid(); // временное решение
            var booking = await _bookingService.CreateBookingAsync(id, userId);
            return AcceptedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
        }

        [HttpGet]
        [Route("bookings/{id:guid}")]
        public async Task<ActionResult<BookingDTO>> GetBookingById(Guid id)
        {
            return Ok(await _bookingService.GetBookingByIdAsync(id));

        }
    }
}
