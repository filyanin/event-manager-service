using BookingService.Application.Interfaces;
using BookingService.Domain.Enum;
using BookingService.Domain.Models;
using BookingService.Infrastructure.DataAssets;
using BookingService.Infrastructure.DataAssets.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookingService.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;
        public BookingRepository(AppDbContext context) => _context = context;

        public async Task<DomainBooking> CreateAsync(DomainBooking booking)
        {
            var bookingModel = new Booking
            {
                Id = booking.Id,
                EventId = booking.EventId,
                UserId = booking.UserId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                ProcessedAt = booking.ProcessedAt
            };

            await _context.Bookings.AddAsync(bookingModel);
            await _context.SaveChangesAsync();

            return bookingModel.ConvertToDomainBooking();
        }

        public async Task<DomainBooking> GetByIdAsync(Guid bookingId)
        {
            var model = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

            if (model == null)
            {
                var ex = new KeyNotFoundException("Booking not found");
                ex.Data["firstParamName"] = nameof(bookingId);
                ex.Data["firstParamValue"] = bookingId;
                throw ex;
            }

            return model.ConvertToDomainBooking();
        }

        public async Task<List<DomainBooking>> GetByStateAsync(BookingStatus state)
        {
            var items = await _context.Bookings.Where(b => b.Status == state).ToListAsync();
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
                var ex = new KeyNotFoundException("Booking not found");
                ex.Data["firstParamName"] = nameof(booking.Id);
                ex.Data["firstParamValue"] = booking.Id;
                throw ex;
            }

            model.Status = booking.Status;
            model.ProcessedAt = booking.ProcessedAt;

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetActiveBookingCountAsync(Guid userId)
        {
            return await _context.Bookings.Where(b => b.UserId == userId && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Rejected).CountAsync();
        }

        public async Task<bool> BookingBelongsToUserAsync(Guid bookingId, Guid userId)
        {
            return await _context.Bookings.AnyAsync(b => b.Id == bookingId && b.UserId == userId);
        }
    }
}
