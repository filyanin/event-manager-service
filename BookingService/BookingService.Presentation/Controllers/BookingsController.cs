using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Domain.Enum;
using System.Security.Claims;

namespace BookingService.Presentation.Controllers;

/// <summary>
/// Контроллер для управления бронированиями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
    }

    /// <summary>
    /// Создаёт новое бронирование
    /// </summary>
    /// <param name="request">Данные для создания бронирования</param>
    /// <returns>Созданное бронирование</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookingDTO>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        if (request == null)
            return BadRequest(new { message = "Booking data is required" });

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid user ID in token" });

            var booking = await _bookingService.CreateBookingAsync(request.EventId, userId, request.SeatsToBook);
            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Получает бронирование по ID
    /// </summary>
    /// <param name="id">ID бронирования</param>
    /// <returns>Данные бронирования</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDTO>> GetBookingById(Guid id)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            return Ok(booking);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Получает бронирования текущего пользователя
    /// </summary>
    /// <returns>Список бронирований</returns>
    [HttpGet("user/my-bookings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookingDTO>>> GetMyBookings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user ID in token" });

        var bookings = await _bookingService.GetBookingsByUserAsync(userId);
        return Ok(bookings);
    }

    /// <summary>
    /// Получает бронирания по статусу
    /// </summary>
    /// <param name="status">Статус бронирования</param>
    /// <returns>Список бронирований с указанным статусом</returns>
    [HttpGet("status/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookingDTO>>> GetBookingsByStatus(BookingStatus status)
    {
        var bookings = await _bookingService.GetBookingByStatusAsync(status);
        return Ok(bookings);
    }

    /// <summary>
    /// Отменяет бронирование
    /// </summary>
    /// <param name="id">ID бронирования</param>
    /// <returns>Статус отмены</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid user ID in token" });

            await _bookingService.CancelBookingAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Подтверждает бронирование (только для администраторов)
    /// </summary>
    /// <param name="id">ID бронирования</param>
    /// <returns>Статус подтверждения</returns>
    [HttpPost("{id}/confirm")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmBooking(Guid id)
    {
        try
        {
            await _bookingService.ConfirmBookingAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Отклоняет бронирование (только для администраторов)
    /// </summary>
    /// <param name="id">ID бронирования</param>
    /// <returns>Статус отклонения</returns>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectBooking(Guid id)
    {
        try
        {
            await _bookingService.RejectBookingAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
