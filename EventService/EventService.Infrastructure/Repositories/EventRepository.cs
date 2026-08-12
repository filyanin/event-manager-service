using EventService.Domain.Filters;
using EventService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using EventService.Domain.Models;
using EventService.Infrastructure.DataAssets.Models;
using EventService.Domain.ValueObjects;
using EventService.Application.Interfaces;

namespace EventService.Infrastructure.Repositories;

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
            CreatedByUserId = ev.CreatedByUserId,
            CreatedAt = DateTime.UtcNow,
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
            throw new KeyNotFoundException($"Event {id} not found");

        _context.Events.Remove(model);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id) => await _context.Events.AnyAsync(e => e.Id == id);

    public async Task<(IList<DomainEvent> Items, int Total)> GetAllAsync(EventsFilters filters, Paginations paginations)
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
        var items = await dbQuery
            .OrderBy(e => e.StartAt)
            .Skip((paginations.pageNumber - 1) * paginations.pageSize)
            .Take(paginations.pageSize)
            .ToListAsync();
        var domainItems = items.Select(i => i.ConvertToDomainEvent()).ToList();
        return (domainItems, total);
    }

    public async Task<DomainEvent> GetByIdAsync(Guid id)
    {
        var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (model == null)
            throw new KeyNotFoundException($"Event {id} not found");

        return model.ConvertToDomainEvent();
    }

    public async Task UpdateAsync(DomainEvent domainEvent)
    {
        var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == domainEvent.Id);
        if (model == null)
            throw new KeyNotFoundException($"Event {domainEvent.Id} not found");

        model.Title = domainEvent.Title;
        model.Description = domainEvent.Description;
        model.StartAt = domainEvent.StartAt;
        model.EndAt = domainEvent.EndAt;
        model.UpdatedAt = DateTime.UtcNow;

        _context.Events.Update(model);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> TryReserveSeatsAsync(DomainEvent @event)
    {
        var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == @event.Id);

        if (model == null)
            throw new KeyNotFoundException($"Event {@event.Id} not found");

        model.Timestamp = @event.Timestamp;
        model.AvailableSeats = @event.AvailableSeats;
        model.UpdatedAt = DateTime.UtcNow;
        try
        {
            _context.Events.Update(model);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("SeatsReserveConcurrencyException");
        }

        return true;
    }

    public async Task<bool> TryReserveSeatsIdempotentAsync(Guid bookingId, Guid eventId, int seatsToReserve)
    {
        // Помечаем bookingId как обработанный до списания мест: уникальный первичный ключ
        // ProcessedBookingConfirmations гарантирует, что повторная доставка (или дублирование)
        // того же сообщения BookingConfirmed не приведёт к повторному уменьшению доступных мест.
        await using var transaction = await _context.Database.BeginTransactionAsync();

        _context.ProcessedBookingConfirmations.Add(new ProcessedBookingConfirmation
        {
            BookingId = bookingId,
            ProcessedAt = DateTime.UtcNow
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Booking с таким Id уже был обработан ранее — сообщение является дубликатом.
            await transaction.RollbackAsync();
            return false;
        }

        // Читаем актуальное состояние события внутри транзакции (а не полагаемся на значение,
        // прочитанное до начала обработки в вызывающем коде), чтобы конкурентные подтверждения
        // применяли свою дельту поверх последних сохранённых данных, а не перезаписывали друг друга.
        var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (model == null)
            throw new KeyNotFoundException($"Event {eventId} not found");

        if (model.AvailableSeats < seatsToReserve)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("NotEnoughAvailableSeats");
        }

        model.AvailableSeats -= seatsToReserve;
        model.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("SeatsReserveConcurrencyException");
        }

        return true;
    }

    public async Task<bool> ReleaseSeatsAsync(DomainEvent @event)
    {
        var model = await _context.Events.FirstOrDefaultAsync(e => e.Id == @event.Id);
        if (model == null)
            throw new KeyNotFoundException($"Event {@event.Id} not found");

        model.AvailableSeats = @event.AvailableSeats;
        model.UpdatedAt = DateTime.UtcNow;

        try
        {
            _context.Events.Update(model);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("HighLoad");
        }

        return true;
    }
}
