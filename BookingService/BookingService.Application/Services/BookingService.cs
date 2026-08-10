using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Domain.Enum;
using BookingService.Domain.Models;

namespace BookingService.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        }

        public async Task<BookingDTO> CreateBookingAsync(Guid eventId, Guid userId, int seatsToReserve = 1)
        {
            // В BookingService нет прямой проверки события — сохраняем только EventId
            var booking = new DomainBooking(eventId, userId);

            var result = await _bookingRepository.CreateAsync(booking);

            return new BookingDTO()
            {
                EventId = result.EventId,
                Id = result.Id,
                Status = result.Status
            };
        }

        public async Task<BookingDTO> GetBookingByIdAsync(Guid bookingId)
        {
            var result = await _bookingRepository.GetByIdAsync(bookingId);
            return new BookingDTO()
            {
                EventId = result.EventId,
                Id = result.Id,
                Status = result.Status
            };
        }

        public async Task<List<BookingDTO>> GetBookingByStateAsync(BookingStatus state)
        {
            var results = await _bookingRepository.GetByStateAsync(state);
            return results.Select(r => new BookingDTO()
            {
                EventId = r.EventId,
                Id = r.Id,
                Status = r.Status
            }).ToList();
        }

        public async Task ConfirmBookingAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            booking.SetBookingConfirmed(DateTime.UtcNow);
            await _bookingRepository.ChangeBookingStateAsync(booking);
        }

        public async Task RejectBookingAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            booking.SetBookingRejected(DateTime.UtcNow);
            await _bookingRepository.ChangeBookingStateAsync(booking);
        }

        public async Task CancelBookingAsync(Guid bookingId, Guid userId, string userRole = "user")
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            bool isAdmin = userRole?.Equals("admin", StringComparison.OrdinalIgnoreCase) ?? false;

            if (booking.UserId != userId && !isAdmin)
            {
                throw new UnauthorizedAccessException("User is not authorized to cancel this booking");
            }

            booking.SetBookingCancelled(DateTime.UtcNow);
            await _bookingRepository.ChangeBookingStateAsync(booking);
        }
    }
}
