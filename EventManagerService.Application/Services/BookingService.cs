using EventManagerService.Application.DTOs;
using EventManagerService.Application.Exceptions;
using EventManagerService.Application.Interfaces;
using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Exceptions;
using EventManagerService.Domain.Models;

namespace EventManagerService.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;

        public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
        {
            _bookingRepository = bookingRepository ?? throw new Shared.Exceptions.AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, nameof(bookingRepository));
            _eventRepository = eventRepository ?? throw new Shared.Exceptions.AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, nameof(eventRepository));
        }

        public async Task<BookingDTO> CreateBookingAsync(Guid eventId, Guid userId, int seatsToReserve = 1)
        {
            if (!await _eventRepository.ExistsAsync(eventId))
            {
                var ex = new Shared.Exceptions.AppException(Shared.ErrorCodes.ErrorCodes.NotFound);
                ex.Data["firstParamName"] = nameof(eventId);
                ex.Data["firstParamValue"] = eventId;
                throw ex;
            }

            var eventEntity = await _eventRepository.GetByIdAsync(eventId);

            // Проверка: запретить бронирование события, которое уже началось
            if (DateTime.UtcNow >= eventEntity.StartAt)
            {
                throw new PastEventBookingException(Shared.ErrorCodes.ErrorCodes.PastEventBookingError);
            }

            // Проверка: ограничить количество активных броней пользователя (максимум 10)
            const int maxActiveBookings = 10;
            var activeBookingCount = await _bookingRepository.GetActiveBookingCountAsync(userId);
            if (activeBookingCount >= maxActiveBookings)
            {
                throw new ActiveBookingsLimitException(
                    Shared.ErrorCodes.ErrorCodes.ActiveBookingsLimitExceededError,
                    activeBookingCount,
                    maxActiveBookings);
            }

            eventEntity.TryReserveSeats(seatsToReserve);

            bool reserved = false;
            try
            {
                reserved = await _eventRepository.TryReserveSeatsAsync(eventEntity);
            }
            catch (SeatsReserveConcurencyException ex)
            {
                //Вторая попытка зарезервировать места, если первая не удалась из-за конкуренции
                Random rnd = new Random();
                await Task.Delay(rnd.Next(50, 201)); //небольшая случайная задержка перед повторной попыткой, чтобы разнести нагрузку равномернее

                eventEntity = await _eventRepository.GetByIdAsync(eventId);

                eventEntity.TryReserveSeats(seatsToReserve);

                try
                {
                    reserved = await _eventRepository.TryReserveSeatsAsync(eventEntity);
                }
                catch (SeatsReserveConcurencyException ex2)
                {
                    var exeption = new HightLoadException("SeatsReserveConcurencyException", ex2);
                    throw exeption;
                }
            }

            if (!reserved)
            {
                var ex = new NoAvailableSeatsException(EventManagerService.Shared.ErrorCodes.ErrorCodes.NoAvailableSeatsError);
                ex.Data["firstParamName"] = nameof(eventId);
                ex.Data["firstParamValue"] = eventId;
                throw ex;
            }

            var booking = new DomainBooking(eventId, userId);

            try
            {
                var result = await _bookingRepository.CreateAsync(booking);

                return new BookingDTO()
                {
                    EventId = result.EventId,
                    Id = result.Id,
                    Status = result.Status
                };
            }
            catch
            {
                eventEntity.ReleaseSeats(seatsToReserve);
                await _eventRepository.ReleaseSeatsAsync(eventEntity);

                throw;
            }
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
            {
                var ex = new EventManagerService.Shared.Exceptions.AppException(EventManagerService.Shared.ErrorCodes.ErrorCodes.NotFound);
                ex.Data["firstParamName"] = nameof(bookingId);
                ex.Data["firstParamValue"] = bookingId;
                throw ex;
            }
            booking.SetBookingConfirmed(DateTime.UtcNow);
            
            await _bookingRepository.ChangeBookingStateAsync(booking);
        }

        public async Task RejectBookingAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            
            if (booking == null)
            {
                var ex = new EventManagerService.Shared.Exceptions.AppException(EventManagerService.Shared.ErrorCodes.ErrorCodes.NotFound);
                ex.Data["firstParamName"] = nameof(bookingId);
                ex.Data["firstParamValue"] = bookingId;
                throw ex;
            }

            booking.SetBookingRejected(DateTime.UtcNow);

            var @event = await _eventRepository.GetByIdAsync(booking.EventId);

            if (@event == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["firstParamName"] = nameof(booking.EventId);
                ex.Data["firstParamValue"] = booking.EventId;
                throw ex;
            }

            @event.ReleaseSeats(1);


            await _bookingRepository.ChangeBookingStateAsync(booking);
            await _eventRepository.ReleaseSeatsAsync(@event);
        }

        public async Task CancelBookingAsync(Guid bookingId, Guid userId, string userRole = "user")
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                var ex = new Shared.Exceptions.AppException(Shared.ErrorCodes.ErrorCodes.NotFound);
                ex.Data["firstParamName"] = nameof(bookingId);
                ex.Data["firstParamValue"] = bookingId;
                throw ex;
            }

            var @event = await _eventRepository.GetByIdAsync(booking.EventId);

            if (@event == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["firstParamName"] = nameof(booking.EventId);
                ex.Data["firstParamValue"] = booking.EventId;
                throw ex;
            }

            bool isAdmin = userRole?.Equals("admin", StringComparison.OrdinalIgnoreCase) ?? false;

            if (@event.StartAt <= DateTime.UtcNow && !isAdmin)
            {
                throw new PastEventBookingException(Shared.ErrorCodes.ErrorCodes.PastEventBookingError);
            }



            // Проверка прав: пользователь может отменить только свою бронь или администратор может отменить любую
            
            if (booking.UserId != userId && !isAdmin)
            {
                throw new UnauthorizedBookingCancellationException(
                    Shared.ErrorCodes.ErrorCodes.UnauthorizedBookingCancellationError,
                    bookingId,
                    userId);
            }

            // Установить статус на Cancelled
            booking.SetBookingCancelled(DateTime.UtcNow);

            @event.ReleaseSeats(1);

            await _bookingRepository.ChangeBookingStateAsync(booking);
            await _eventRepository.ReleaseSeatsAsync(@event);
        }
    }
}
