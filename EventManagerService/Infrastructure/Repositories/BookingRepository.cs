using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models.DomainBooking;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using EventManagerService.Properties;
using EventManagerService.Infrastructure.Interfaces.Repositories;

namespace EventManagerService.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;
        public BookingRepository(AppDbContext context) => _context = context;

        // Создаёт запись брони; ожидается, что проверка и резервирование мест выполнены на уровне сервиса
        public async Task<DomainBooking> CreateAsync(Guid eventId)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (model == null) throw new KeyNotFoundException(string.Format(ErrorMessages.ObjectNotFound, eventId));

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

        public async Task<DomainBooking> GetByIdAsync(Guid bookingId)
        {
            var model = await _context.Bookings.Include(b => b.Event).FirstOrDefaultAsync(b => b.Id == bookingId);
            if (model == null) throw new KeyNotFoundException();
            return model.ConvertTo();
        }

        public async Task<List<DomainBooking>> GetByStateAsync(BookingStatus state)
        {
            var items = await _context.Bookings.Include(b => b.Event).Where(b => b.Status == state).ToListAsync();
            return items.Select(i => i.ConvertTo()).ToList();
        }

        public async Task ConfirmAsync(Guid bookingId)
        {
            var model = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (model == null) throw new KeyNotFoundException();

            var domainBooking = model.ConvertTo();
            domainBooking.SetBookingConfirmed(DateTime.UtcNow);

            model.Status = domainBooking.Status;
            model.ProcessedAt = domainBooking.ProcessedAt;

            _context.Bookings.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task RejectAsync(Guid bookingId)
        {
            var model = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (model == null) throw new KeyNotFoundException();

            var domainBooking = model.ConvertTo();
            domainBooking.SetBookingRejected(DateTime.UtcNow);

            model.Status = domainBooking.Status;
            model.ProcessedAt = domainBooking.ProcessedAt;

            _context.Bookings.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Guid>> GetPendingIdsAsync(CancellationToken cancellationToken)
        {
            return await _context.Bookings.Where(b => b.Status == BookingStatus.Pending).Select(b => b.Id).ToListAsync(cancellationToken);
        }
    }
}
