using BookingService.Domain.Models;
using BookingService.Domain.Enum;

namespace BookingService.Infrastructure.DataAssets.Models;

public class Booking
{
    public Guid Id { get; set; }

    public Guid EventGuid { get; set; }

    public Guid UserGuid { get; set; }

    public int SeatsBooked { get; set; }

    public BookingStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public Booking() { }

    public DomainBooking ConvertToDomainBooking()
    {
        return new DomainBooking(Id, EventGuid, UserGuid, SeatsBooked, Status, CreatedAt, ProcessedAt);
    }
}
