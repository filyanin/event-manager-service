using EventManagerService.Application.DTOs;
using EventManagerService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventManagerService.Controllers
{

    [ApiController]
    [Authorize]
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
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BookingDTO>> CreateBooking(Guid id)
        {
            // Получаем userId из claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing user ID in token" });
            }

            var booking = await _bookingService.CreateBookingAsync(id, userId);
            return AcceptedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
        }

        [HttpGet]
        [Route("bookings/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BookingDTO>> GetBookingById(Guid id)
        {
            return Ok(await _bookingService.GetBookingByIdAsync(id));
        }

        [HttpDelete]
        [Route("bookings/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteBooking(Guid id)
        {
            // Получаем userId из claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing user ID in token" });
            }

            // Получаем роль пользователя из claims
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "user";

            await _bookingService.CancelBookingAsync(id, userId, userRole);
            return Ok();
        }
    }
}
