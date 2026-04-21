using EventManagerService.Domain.Enum;
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

        public BookingService(IEventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<Booking> CreateBookingAsync(Guid eventId)
        {
            if (!await _eventService.CheckEventById(eventId))
            {
                throw new KeyNotFoundException(string.Format(
                    new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), eventId));
            }
                        
            var booking = new Booking(eventId);

            bookings.Add(booking);
            return booking;               

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
