using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models;
using EventManagerService.Infrastructure.DataAssets;
using EventManagerService.Infrastructure.DataAssets.Models;
using EventManagerService.Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventManagerService.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;
        public BookingRepository(AppDbContext context) => _context = context;



        public async Task<DomainBooking> CreateAsync(DomainBooking booking)
        {
            var model = await _context.Events.FindAsync(booking.EventId);

            if (model == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["FirstParamName"] = nameof(booking.EventId);
                ex.Data["FirstParamValue"] = booking.EventId;

                throw ex;
            }

            var bookingModel = new Infrastructure.DataAssets.Models.Booking
            {
                Id =booking.Id,
                EventId = booking.EventId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                ProcessedAt = booking.ProcessedAt,
                Event = model
            };

            await _context.Bookings.AddAsync(bookingModel);
            await _context.SaveChangesAsync();

            return bookingModel.ConvertToDomainBooking();
        }

        public async Task<DomainBooking> GetByIdAsync(Guid bookingId)
        {
            var model = await _context.Bookings.Include(b => b.Event).FirstOrDefaultAsync(b => b.Id == bookingId);
            
            if (model == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["FirstParamName"] = nameof(bookingId);
                ex.Data["FirstParamValue"] = bookingId;

                throw ex;
            }

            return model.ConvertToDomainBooking();
        }

        public async Task<List<DomainBooking>> GetByStateAsync(BookingStatus state)
        {
            var items = await _context.Bookings.Include(b => b.Event).Where(b => b.Status == state).ToListAsync();

            return items.Select(i => i.ConvertToDomainBooking()).ToList();
        }


        public async Task<List<Guid>> GetPendingIdsAsync(CancellationToken cancellationToken)
        {
            return await _context.Bookings.Where(b => b.Status == BookingStatus.Pending).Select(b => b.Id).ToListAsync(cancellationToken);
        }



        public async Task ChangeBookingStateAsync(DomainBooking booking)
        {
            var model = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);

            if (model == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["FirstParamName"] = nameof(booking.Id);
                ex.Data["FirstParamValue"] = booking.Id;

                throw ex;
            }

            model.Status = booking.Status;
            model.ProcessedAt = booking.ProcessedAt;

            await _context.SaveChangesAsync();
        }
    }
}
