using BookingService.Domain.Enum;

namespace BookingService.Domain.Models;

public class DomainBooking
{
    public Guid Id { get; private set; }

    public Guid EventGuid { get; private set; }

    public Guid UserGuid { get; private set; }

    public int SeatsBooked { get; private set; }

    public BookingStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public DomainBooking(Guid eventGuid, Guid userGuid, int seatsBooked = 1)
    {
        if (seatsBooked <= 0)
            throw new ArgumentException("SeatsBooked must be greater than 0", nameof(seatsBooked));

        Id = Guid.NewGuid();
        EventGuid = eventGuid;
        UserGuid = userGuid;
        SeatsBooked = seatsBooked;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        ProcessedAt = null;
    }

    public DomainBooking(Guid id, Guid eventGuid, Guid userGuid, int seatsBooked, BookingStatus status, DateTime createdAt, DateTime? processedAt)
    {
        if (seatsBooked <= 0)
            throw new ArgumentException("SeatsBooked must be greater than 0", nameof(seatsBooked));

        if (processedAt != null && createdAt >= processedAt)
        {
            throw new ArgumentException("processedAt must be greater than createdAt");
        }

        Id = id;
        EventGuid = eventGuid;
        UserGuid = userGuid;
        SeatsBooked = seatsBooked;
        Status = status;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;
    }

    public void SetBookingConfirmed(DateTime processedAt)
    {
        if (!Status.Equals(BookingStatus.Pending))
            throw new InvalidOperationException("Cannot confirm non-pending booking");

        if (CreatedAt >= processedAt)
            throw new ArgumentException("processedAt must be greater than CreatedAt");

        Status = BookingStatus.Confirmed;
        ProcessedAt = processedAt;
    }

    public void SetBookingRejected(DateTime rejectedAt)
    {
        if (!Status.Equals(BookingStatus.Pending))
            throw new InvalidOperationException("Cannot reject non-pending booking");

        if (CreatedAt >= rejectedAt)
            throw new ArgumentException("rejectedAt must be greater than CreatedAt");

        Status = BookingStatus.Rejected;
        ProcessedAt = rejectedAt;
    }

    public void SetBookingCancelled(DateTime cancelledAt)
    {
        if (!Status.Equals(BookingStatus.Pending) && !Status.Equals(BookingStatus.Confirmed))
            throw new InvalidOperationException("Cannot cancel booking in its current state");

        if (CreatedAt >= cancelledAt)
            throw new ArgumentException("cancelledAt must be greater than CreatedAt");

        Status = BookingStatus.Cancelled;
        ProcessedAt = cancelledAt;
    }
}
