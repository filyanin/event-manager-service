namespace Shared.Contracts.Events.Event;

public class EventCreatedEvent : IntegrationEvent
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public Guid CreatedByUserId { get; set; }

    public EventCreatedEvent() { }

    public EventCreatedEvent(Guid eventId, string title, DateTime startAt, DateTime endAt, int totalSeats, Guid createdByUserId) 
        : base(eventId)
    {
        EventId = eventId;
        Title = title;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
        CreatedByUserId = createdByUserId;
    }
}

public class EventUpdatedEvent : IntegrationEvent
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }

    public EventUpdatedEvent() { }

    public EventUpdatedEvent(Guid eventId, string title, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats) 
        : base(eventId)
    {
        EventId = eventId;
        Title = title;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = availableSeats;
    }
}

public class EventDeletedEvent : IntegrationEvent
{
    public Guid EventId { get; set; }

    public EventDeletedEvent() { }

    public EventDeletedEvent(Guid eventId) : base(eventId)
    {
        EventId = eventId;
    }
}

public class EventSeatsReservedEvent : IntegrationEvent
{
    public Guid EventId { get; set; }
    public int ReservedSeats { get; set; }
    public int RemainingSeats { get; set; }

    public EventSeatsReservedEvent() { }

    public EventSeatsReservedEvent(Guid eventId, int reservedSeats, int remainingSeats) : base(eventId)
    {
        EventId = eventId;
        ReservedSeats = reservedSeats;
        RemainingSeats = remainingSeats;
    }
}
