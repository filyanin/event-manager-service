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

        // Репозиторий не содержит сложной бизнес-логики резервирования мест
        // Эти методы оставлены как простые операции изменения числа доступных мест
        public async Task<bool> TryReserveSeatsAsync(Guid id, int count = 1)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (model == null) throw new KeyNotFoundException(string.Format(ErrorMessages.ObjectNotFound, id));
            if (model.AvailableSeats - count < 0) return false;
            model.AvailableSeats -= count;
            _context.Events.Update(model);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReleaseSeatsAsync(Guid id, int count = 1)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (model == null) throw new KeyNotFoundException(string.Format(ErrorMessages.ObjectNotFound, id));
            if (model.AvailableSeats + count > model.TotalSeats) return false;
            model.AvailableSeats += count;
            _context.Events.Update(model);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
