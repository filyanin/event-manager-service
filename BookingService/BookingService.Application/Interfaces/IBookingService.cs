using BookingService.Application.DTOs;
using BookingService.Domain.Enum;

namespace BookingService.Application.Interfaces;

public interface IBookingService
{
    public Task<BookingDTO> CreateBookingAsync(Guid eventGuid, Guid userGuid, int seatsToBook = 1);

    public Task<BookingDTO> GetBookingByIdAsync(Guid bookingId);

    public Task<List<BookingDTO>> GetBookingsByEventAsync(Guid eventGuid);

    public Task<List<BookingDTO>> GetBookingsByUserAsync(Guid userGuid);

    public Task<List<BookingDTO>> GetBookingByStatusAsync(BookingStatus status);

    public Task ConfirmBookingAsync(Guid bookingId);

    public Task RejectBookingAsync(Guid bookingId);

    public Task CancelBookingAsync(Guid bookingId, Guid userGuid);

    public Task<int> GetActiveBookingsCountAsync(Guid userGuid);
}
