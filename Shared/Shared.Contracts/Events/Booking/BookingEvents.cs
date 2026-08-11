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

/// <summary>
/// Неизменяемый контракт события подтверждения брони.
/// Публикуется BookingService и потребляется EventService по топику <see cref="Topics.KafkaTopics.BookingConfirmed"/>.
/// </summary>
public sealed class BookingConfirmedEvent : IntegrationEvent
{
    public Guid BookingId { get; }
    public Guid EventGuid { get; }
    public Guid UserGuid { get; }
    public int SeatsBooked { get; }
    public DateTime ConfirmedAt { get; }

    public BookingConfirmedEvent(Guid bookingId, Guid eventGuid, Guid userGuid, int seatsBooked, DateTime confirmedAt) : base(bookingId)
    {
        BookingId = bookingId;
        EventGuid = eventGuid;
        UserGuid = userGuid;
        SeatsBooked = seatsBooked;
        ConfirmedAt = confirmedAt;
    }
}
