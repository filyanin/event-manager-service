using EventManagerService.Application.DTOs;
using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models;

namespace EventManagerService.Application.Interfaces
{
    public interface IBookingService
    {
        public Task<BookingDTO> CreateBookingAsync(Guid eventId, Guid userId, int seatsToReserve = 1);

        public Task<BookingDTO> GetBookingByIdAsync(Guid bookingId);

        public Task<List<BookingDTO>> GetBookingByStateAsync(BookingStatus state);

        public Task ConfirmBookingAsync(Guid bookingId);

        public Task RejectBookingAsync(Guid bookingId);

        /// <summary>
        /// Отменяет бронирование пользователя
        /// </summary>
        /// <param name="bookingId">Идентификатор бронирования</param>
        /// <param name="userId">Идентификатор текущего пользователя</param>
        /// <param name="userRole">Роль текущего пользователя (для проверки прав доступа)</param>
        public Task CancelBookingAsync(Guid bookingId, Guid userId, string userRole = "user");
    }
}
