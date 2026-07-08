using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Interfaces.Repositories;
using EventManagerService.Domain.Models.DomainEvent;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using EventManagerService.Properties;
using System.Linq;

namespace EventManagerService.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context) => _context = context;

        public async Task<DomainEvent> AddAsync(DomainEvent ev)
        {
            var model = ev.ConvertTo();
            await _context.Events.AddAsync(model);
            await _context.SaveChangesAsync();
            return model.ConvertTo();
        }

        public async Task DeleteAsync(Guid id)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (model == null)
                throw new KeyNotFoundException(string.Format(ErrorMessages.ObjectNotFound, id));
            _context.Events.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id) => await _context.Events.AnyAsync(e => e.Id == id);

        public async Task<(IReadOnlyList<DomainEvent> Items, int Total)> GetAllAsync(EventsFilters filters, int page, int pageSize)
        {
            if (page < 1) throw new ArgumentException();
            if (pageSize < 10 || pageSize > 100) throw new ArgumentException();

            IQueryable<Infrastructure.DataAssets.Models.Event> dbQuery = _context.Events;

            if (!string.IsNullOrEmpty(filters.Title))
            {
                var title = filters.Title.ToLower();
                dbQuery = dbQuery.Where(e => e.Title.ToLower().Contains(title));
            }
            if (filters.From != null)
            {
                dbQuery = dbQuery.Where(e => e.StartAt >= filters.From.Value);
            }
            if (filters.To != null)
            {
                dbQuery = dbQuery.Where(e => e.EndAt <= filters.To.Value);
            }

            var total = await dbQuery.CountAsync();
            var items = await dbQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var domainItems = items.Select(i => i.ConvertTo()).ToList().AsReadOnly();
            return (domainItems, total);
        }

        public async Task<DomainEvent> GetByIdAsync(Guid id)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (model == null)
                throw new KeyNotFoundException(string.Format(ErrorMessages.ObjectNotFound, id));
            return model.ConvertTo();
        }

        public async Task UpdateAsync(DomainEvent domainEvent)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == domainEvent.Id);
            if (model == null) throw new KeyNotFoundException(string.Format(ErrorMessages.ObjectNotFound, domainEvent.Id));

            model.Title = domainEvent.Title;
            model.Description = domainEvent.Description;
            model.StartAt = domainEvent.StartAt;
            model.EndAt = domainEvent.EndAt;

            _context.Events.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TryReserveSeatsAsync(Guid id, int count = 1)
        {
            // Попытка атомарно уменьшить AvailableSeats на сервере, чтобы избежать гонок при высокой конкуренции.
            // Используем ExecuteUpdateAsync — выполнится одним SQL UPDATE и вернёт количество изменённых строк.
            var affected = await _context.Events
                .Where(e => e.Id == id && e.AvailableSeats >= count)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.AvailableSeats, e => e.AvailableSeats - count));

            if (affected == 0)
            {
                // Проверим, существует ли событие — если нет, бросим KeyNotFoundException
                var exists = await _context.Events.AnyAsync(e => e.Id == id);
                if (!exists) throw new KeyNotFoundException(string.Format(ErrorMessages.ObjectNotFound, id));
                return false;
            }

            return true;
        }

        public async Task<bool> ReleaseSeatsAsync(Guid id, int count = 1)
        {
            // Попытка атомарно увеличить AvailableSeats, но не превысить TotalSeats
            var affected = await _context.Events
                .Where(e => e.Id == id && e.AvailableSeats + count <= e.TotalSeats)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.AvailableSeats, e => e.AvailableSeats + count));

            if (affected == 0)
            {
                var exists = await _context.Events.AnyAsync(e => e.Id == id);
                if (!exists) throw new KeyNotFoundException(string.Format(ErrorMessages.ObjectNotFound, id));
                return false;
            }

            return true;
        }
    }
}
