using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Exceptions;
using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Models.Booking;
using EventManagerService.Properties;
using System.Resources;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace EventManagerService.Domain.Services.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;
        private static readonly SemaphoreSlim _bookingSemaphore = new SemaphoreSlim(1, 1);

        public BookingService(AppDbContext context) =>
            _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<Booking> CreateBookingAsync(Guid eventId)
        {
            await _bookingSemaphore.WaitAsync();
            try
            {
                var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
                if (model == null)
                {
                    throw new KeyNotFoundException(string.Format(
                        new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), eventId));
                }

                if (model.AvailableSeats <= 0)
                {
                    throw new NoAvailableSeatsException("No available seats for this event");
                }

                model.AvailableSeats -= 1;

                var bookingModel = new Infrastructure.DataAssets.Models.Booking
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Event = model
                };

                await _context.Bookings.AddAsync(bookingModel);
                await _context.SaveChangesAsync();

                return bookingModel.ConvertTo();
            }
            finally
            {
                _bookingSemaphore.Release();
            }
        }

        public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            var model = await _context.Bookings.Include(b => b.Event).FirstOrDefaultAsync(b => b.Id == bookingId);

            if (model == null)
            {
                throw new KeyNotFoundException(string.Format(
                   new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), bookingId));
            }

            return model.ConvertTo();
        }

        public async Task<List<Booking>> GetBookingByStateAsync(BookingStatus state)
        {
            var items = await _context.Bookings.Include(b => b.Event).Where(b => b.Status == state).ToListAsync();
            return items.Select(i => i.ConvertTo()).ToList();
        }

        public async Task ConfirmBookingAsync(Guid bookingId)
        {
            var model = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

            if (model == null)
            {
                throw new KeyNotFoundException(string.Format(
                   new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), bookingId));
            }

            var domainBooking = model.ConvertTo();
            domainBooking.SetBookingConfirmed(DateTime.UtcNow);

            model.Status = domainBooking.Status;
            model.ProcessedAt = domainBooking.ProcessedAt;

            _context.Bookings.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task RejectBookingAsync(Guid bookingId)
        {
            var model = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

            if (model == null)
            {
                throw new KeyNotFoundException(string.Format(
                   new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), bookingId));
            }

            var domainBooking = model.ConvertTo();
            domainBooking.SetBookingRejected(DateTime.UtcNow);

            model.Status = domainBooking.Status;
            model.ProcessedAt = domainBooking.ProcessedAt;

            _context.Bookings.Update(model);
            await _context.SaveChangesAsync();
        }
    }
}
