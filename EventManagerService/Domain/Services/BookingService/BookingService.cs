using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Exceptions;
using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Models.Booking;
using EventManagerService.Properties;
using System.Resources;

namespace EventManagerService.Domain.Services.BookingService
{
    public class BookingService : IBookingService
    {
        private List<Booking> bookings = new List<Booking>();
        private IEventService _eventService;
        private readonly object _bookingLock = new();

        public BookingService(IEventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<Booking> CreateBookingAsync(Guid eventId)
        {
            if (!await _eventService.CheckEventByIdAsync(eventId))
            {
                throw new KeyNotFoundException(string.Format(
                    new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), eventId));
            }

            var @event = await _eventService.GetEventByIdAsync(eventId);

            if (@event == null)
            {
                throw new KeyNotFoundException(string.Format(
                    new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), eventId));
            }

            lock (_bookingLock)
            {
                if (!@event.TryReserveSeats())
                {
                    throw new NoAvailableSeatsException("No available seats for this event");
                }

                var booking = new Booking(eventId);
                bookings.Add(booking);
                return booking;
            }
        }

        public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            int index = await Task.FromResult(bookings.FindIndex(b => b.Id == bookingId));
      
            if (index == -1)
            {
                throw new KeyNotFoundException(string.Format(
                   new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), bookingId));
            }

            return bookings[index];
        }

        public async Task<List<Booking>> GetBookingByStateAsync(BookingStatus state)
        {
            return await Task.FromResult(bookings.Where(b => b.Status == state).ToList());
        }

        public async Task ConfirmBookingAsync(Guid bookingId)
        {
            int index = await Task.FromResult(bookings.FindIndex(b => b.Id == bookingId));

            if (index == -1)
            {
                throw new KeyNotFoundException(string.Format(
                   new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), bookingId));
            }

            bookings[index].SetBookingConfirmed(DateTime.UtcNow);
        }

        public async Task RejectBookingAsync(Guid bookingId)
        {
            int index = await Task.FromResult(bookings.FindIndex(b => b.Id == bookingId));

            if (index == -1)
            {
                throw new KeyNotFoundException(string.Format(
                   new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), bookingId));
            }

            bookings[index].SetBookingRejected(DateTime.UtcNow);
        }
    }
}
