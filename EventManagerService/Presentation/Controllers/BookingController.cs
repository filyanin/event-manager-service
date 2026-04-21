using EventManagerService.Application.Interfaces.BookingService;
using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Models.Booking;
using EventManagerService.Presentation.DTOs.BookingService;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerService.Presentation.Controllers
{

    [ApiController]
    public class BookingController : ControllerBase
    {
        IBookingQueryMapper _bookingQueryMapper;

        public BookingController(IBookingQueryMapper bookingQueryMapper)
        {
            _bookingQueryMapper = bookingQueryMapper;
        }

        [HttpPost]
        [Route("events/{id:guid}/book")]
        public async Task<ActionResult<BookingDTO>> CreateBooking(Guid id)
        {
            var booking = await _bookingQueryMapper.CreateBookingAsync(id);
            return AcceptedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
        }

        [HttpGet]
        [Route("bookings/{id:guid}")]
        public async Task<ActionResult<BookingDTO>> GetBookingById(Guid id)
        {
            return Ok(await _bookingQueryMapper.GetBookingByIdAsync(id));

        }
    }
}
