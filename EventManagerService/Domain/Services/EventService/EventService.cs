using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Properties;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using DomainEvent = EventManagerService.Domain.Models.Event.Event;
using DataEvent = EventManagerService.Infrastructure.DataAssets.Models.Event;

namespace EventManagerService.Domain.Services.EventService
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IReadOnlyList<DomainEvent> Items, int Total)> GetAllEventAsync(EventsFilters filters, int page, int pageSize)
        {
            var tuple = await GetAllEventAsync_Internal(filters, page, pageSize);
            return (tuple.list, tuple.total);
        }

        public async Task<DomainEvent> AddEventAsync(string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null)
        {
            var ev = DomainEvent.Create(title, startAt, endAt, totalSeats, description);

            var model = ev.ConvertTo();
            await _context.Events.AddAsync(model);
            await _context.SaveChangesAsync();
            return model.ConvertTo();
        }

        public void DeleteEvent(Guid id)
        {
            DeleteEventAsync(id).GetAwaiter().GetResult();
        }

        public async Task DeleteEventAsync(Guid id)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (model == null)
            {
                throw new KeyNotFoundException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), id));
            }
            _context.Events.Remove(model);
            await _context.SaveChangesAsync();
            return;
        }

        public async Task<(IReadOnlyList<DomainEvent> list, int total)> GetAllEventAsync_Internal(EventsFilters filters, int page, int pageSize)
        {
            if (page < 1)
            {
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("PageNumberException"), page));
            }

            if (pageSize < 10 || pageSize > 100)
            {
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("PageSizeException"), pageSize));
            }

            IQueryable<DataEvent> dbQuery = _context.Events;

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

        public async Task<DomainEvent> GetEventByIdAsync(Guid id)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (model == null)
            {
                throw new KeyNotFoundException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), id));
            }
            return model.ConvertTo();
        }

        public async Task UpdateEventAsync(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (model == null)
            {
                throw new KeyNotFoundException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("ObjectNotFound"), id));
            }
            // validate using domain logic
            var temp = new DomainEvent(model.Id, model.Title, model.StartAt, model.EndAt, model.TotalSeats, model.AvailableSeats, model.Description);
            temp.UpdateEvent(title, startAt, endAt, description);

            model.Title = title;
            model.Description = description;
            model.StartAt = startAt;
            model.EndAt = endAt;

            _context.Events.Update(model);
            await _context.SaveChangesAsync();
            return;
        }

        public async Task<bool> CheckEventByIdAsync(Guid id)
        {
            return await _context.Events.AnyAsync(e => e.Id == id);
        }
    }
}
