using BookingService.Application.Interfaces;
using BookingService.Domain.Enum;
using BookingService.Domain.Models;
using BookingService.Infrastructure.DataAssets;
using BookingService.Infrastructure.DataAssets.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;
    public BookingRepository(AppDbContext context) => _context = context;

    public async Task<DomainBooking> CreateAsync(DomainBooking booking)
    {
        var bookingModel = new Booking
        {
            Id = booking.Id,
            EventGuid = booking.EventGuid,
            UserGuid = booking.UserGuid,
            SeatsBooked = booking.SeatsBooked,
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
            throw new KeyNotFoundException($"Booking with id {bookingId} not found");
        }

        return model.ConvertToDomainBooking();
    }

    public async Task<List<DomainBooking>> GetByEventGuidAsync(Guid eventGuid)
    {
        var items = await _context.Bookings
            .Where(b => b.EventGuid == eventGuid)
            .ToListAsync();
        return items.Select(i => i.ConvertToDomainBooking()).ToList();
    }

    public async Task<List<DomainBooking>> GetByUserGuidAsync(Guid userGuid)
    {
        var items = await _context.Bookings
            .Where(b => b.UserGuid == userGuid)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return items.Select(i => i.ConvertToDomainBooking()).ToList();
    }

    public async Task<List<DomainBooking>> GetByStatusAsync(BookingStatus status)
    {
        var items = await _context.Bookings.Where(b => b.Status == status).ToListAsync();
        return items.Select(i => i.ConvertToDomainBooking()).ToList();
    }

    public async Task<List<Guid>> GetPendingIdsAsync(CancellationToken cancellationToken)
    {
        return await _context.Bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task ChangeBookingStateAsync(DomainBooking booking)
    {
        var model = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);

        if (model == null)
        {
            throw new KeyNotFoundException($"Booking with id {booking.Id} not found");
        }

        model.Status = booking.Status;
        model.ProcessedAt = booking.ProcessedAt;

        _context.Bookings.Update(model);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetActiveBookingCountAsync(Guid userGuid)
    {
        return await _context.Bookings.Where(b => 
            b.UserGuid == userGuid && 
            b.Status != BookingStatus.Cancelled && 
            b.Status != BookingStatus.Rejected).CountAsync();
    }

    public async Task<bool> BookingBelongsToUserAsync(Guid bookingId, Guid userGuid)
    {
        return await _context.Bookings.AnyAsync(b => b.Id == bookingId && b.UserGuid == userGuid);
    }

    public async Task<int> GetTotalSeatsBookedForEventAsync(Guid eventGuid)
    {
        return await _context.Bookings
            .Where(b => b.EventGuid == eventGuid && 
                   (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
            .SumAsync(b => b.SeatsBooked);
    }

    public async Task DeleteAsync(Guid bookingId)
    {
        var model = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (model != null)
        {
            _context.Bookings.Remove(model);
            await _context.SaveChangesAsync();
        }
    }
}
