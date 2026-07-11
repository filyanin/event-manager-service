using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Exceptions;
using EventManagerService.Infrastructure.Interfaces.Repositories;
using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Models;

namespace EventManagerService.Domain.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;

        public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public async Task<DomainBooking> CreateBookingAsync(Guid eventId, int seatsToReserve = 1)
        {

            if (!await _eventRepository.ExistsAsync(eventId))
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["firstParamName"] = nameof(eventId);
                ex.Data["firstParamValue"] = eventId;
                throw ex;
            }
            var eventEntity = await _eventRepository.GetByIdAsync(eventId);

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
                    throw new HightLoadException("HightLoad");
                }

            }

            if (!reserved)
            {
                var ex = new NoAvailableSeatsException("NoAvailableSeatsError");
                ex.Data["firstParamName"] = nameof(eventId);
                ex.Data["firstParamValue"] = eventId;
                throw ex;
            }

            var booking = new DomainBooking(
                Guid.NewGuid(),
                eventId,
                BookingStatus.Pending,
                DateTime.UtcNow,
                null);

            try
            {                
                return await _bookingRepository.CreateAsync(booking);
            }
            catch
            {

                eventEntity.ReleaseSeats(seatsToReserve);
                await _eventRepository.ReleaseSeatsAsync(eventEntity);

                throw;
            }
        }

        public async Task<DomainBooking> GetBookingByIdAsync(Guid bookingId)
        {
            return await _bookingRepository.GetByIdAsync(bookingId);
        }

        public async Task<List<DomainBooking>> GetBookingByStateAsync(BookingStatus state)
        {
            return await _bookingRepository.GetByStateAsync(state);
        }

        public async Task ConfirmBookingAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
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
                var ex = new KeyNotFoundException("ObjectNotFound");
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
    }
}
