namespace Shared.Contracts.Events.Booking;

public class BookingCreatedEvent : IntegrationEvent
{
    public Guid BookingId { get; set; }
    public Guid UserGuid { get; set; }
    public Guid EventGuid { get; set; }
    public int SeatsBooked { get; set; }

    public BookingCreatedEvent() { }

    public BookingCreatedEvent(Guid bookingId, Guid userGuid, Guid eventGuid, int seatsBooked) : base(bookingId)
    {
        BookingId = bookingId;
        UserGuid = userGuid;
        EventGuid = eventGuid;
        SeatsBooked = seatsBooked;
    }
}

public class BookingCancelledEvent : IntegrationEvent
{
    public Guid BookingId { get; set; }
    public Guid EventGuid { get; set; }
    public int SeatsReleased { get; set; }

    public BookingCancelledEvent() { }

    public BookingCancelledEvent(Guid bookingId, Guid eventGuid, int seatsReleased) : base(bookingId)
    {
        BookingId = bookingId;
        EventGuid = eventGuid;
        SeatsReleased = seatsReleased;
    }
}

public class BookingExpiredEvent : IntegrationEvent
{
    public Guid BookingId { get; set; }
    public Guid EventGuid { get; set; }
    public int SeatsReleased { get; set; }

    public BookingExpiredEvent() { }

    public BookingExpiredEvent(Guid bookingId, Guid eventGuid, int seatsReleased) : base(bookingId)
    {
        BookingId = bookingId;
        EventGuid = eventGuid;
        SeatsReleased = seatsReleased;
    }
}

public class BookingConfirmedEvent : IntegrationEvent
{
    public Guid BookingId { get; set; }
    public Guid EventGuid { get; set; }
    public int SeatsBooked { get; set; }

    public BookingConfirmedEvent() { }

    public BookingConfirmedEvent(Guid bookingId, Guid eventGuid, int seatsBooked) : base(bookingId)
    {
        BookingId = bookingId;
        EventGuid = eventGuid;
        SeatsBooked = seatsBooked;
    }
}
