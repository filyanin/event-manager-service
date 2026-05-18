using EventManagerService.Application.Interfaces.BookingService;
using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Presentation.DTOs.BookingService;

namespace EventManagerService.Application.Services
{
    public class BookingQueryMapper : IBookingQueryMapper
    {
        private IBookingService _bookingService;

        public BookingQueryMapper(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task<BookingDTO> CreateBookingAsync(Guid eventId)
        {
            var booking = await _bookingService.CreateBookingAsync(eventId);

            return new BookingDTO
            {
                Id = booking.Id,
                EventId = booking.EventId,
                Status = booking.Status
            };
        }

        public async Task<BookingDTO> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);

            return new BookingDTO
            {
                Id = booking.Id,
                EventId = booking.EventId,
                Status = booking.Status
            };
        }
    }
}
