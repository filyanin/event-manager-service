using EventManagerService.Domain.Filters;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using EventManagerService.Infrastructure.Interfaces.Repositories;
using EventManagerService.Domain.Models;
using EventManagerService.Infrastructure.DataAssets.Models;
using EventManagerService.Domain.ValueObjects;
using EventManagerService.Domain.Exceptions;

namespace EventManagerService.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context) => _context = context;

        public async Task<DomainEvent> AddAsync(DomainEvent ev)
        {

            var model = new Event
            {
                Id = ev.Id,
                Title = ev.Title,
                StartAt = ev.StartAt,
                EndAt = ev.EndAt,
                TotalSeats = ev.TotalSeats,
                AvailableSeats = ev.AvailableSeats,
                Description = ev.Description
            };

            await _context.Events.AddAsync(model);
            await _context.SaveChangesAsync();

            return model.ConvertToDomainEvent();
        }

        public async Task DeleteAsync(Guid id)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (model == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["FirstParamName"] = nameof(id);
                ex.Data["FirstParamValue"] = id;
                throw ex;
            }

            _context.Events.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id) => await _context.Events.AnyAsync(e => e.Id == id);

        public async Task<(IReadOnlyList<DomainEvent> Items, int Total)> GetAllAsync(EventsFilters filters, Paginations paginations)
        {

            IQueryable<Event> dbQuery = _context.Events;

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
            var items = await dbQuery.Skip((paginations.pageNumber - 1) * paginations.pageSize).Take(paginations.pageSize).ToListAsync();
            var domainItems = items.Select(i => i.ConvertToDomainEvent()).ToList().AsReadOnly();
            return (domainItems, total);
        }

        public async Task<DomainEvent> GetByIdAsync(Guid id)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (model == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["FirstParamName"] = nameof(id);
                ex.Data["FirstParamValue"] = id;
                throw ex;
            }

            return model.ConvertToDomainEvent();
        }

        public async Task UpdateAsync(DomainEvent domainEvent)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == domainEvent.Id);
            if (model == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["FirstParamName"] = nameof(domainEvent.Id);
                ex.Data["FirstParamValue"] = domainEvent.Id;
                throw ex;
            }

            model.Title = domainEvent.Title;
            model.Description = domainEvent.Description;
            model.StartAt = domainEvent.StartAt;
            model.EndAt = domainEvent.EndAt;

            _context.Events.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TryReserveSeatsAsync(DomainEvent @event)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == @event.Id);

            if (model == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["FirstParamName"] = nameof(@event.Id);
                ex.Data["FirstParamValue"] = @event.Id;
                throw ex;
            }

            model.Timestamp = @event.Timestamp;

            model.AvailableSeats = @event.AvailableSeats;
            try
            {
                _context.Events.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var ex = new SeatsReserveConcurencyException("SeatsReserveConcurencyException");
                ex.Data["FirstParamName"] = nameof(@event.Id);
                ex.Data["FirstParamValue"] = @event.Id;
                throw ex;
            }

            return true;          

        }

        public async Task<bool> ReleaseSeatsAsync(DomainEvent @event)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == @event.Id);
            if (model == null)
            {
                var ex = new KeyNotFoundException("ObjectNotFound");
                ex.Data["FirstParamName"] = nameof(@event.Id);
                ex.Data["FirstParamValue"] = @event.Id;
                throw ex;
            }

            model.AvailableSeats = @event.AvailableSeats;

            try
            {
                _context.Events.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) { 
                throw new HightLoadException("HightLoadException");
            }

            return true;
        }
    }
}
