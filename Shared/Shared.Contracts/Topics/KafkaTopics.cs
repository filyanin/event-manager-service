namespace Shared.Contracts.Topics;

public static class KafkaTopics
{
    // Booking события
    public const string BookingCreated = "booking-created";
    public const string BookingConfirmed = "booking-confirmed";
    public const string BookingCancelled = "booking-cancelled";
    public const string BookingExpired = "booking-expired";

    public const string BookingSeatsRejected = "booking-seats-rejected";

    // Event события
    public const string EventCreated = "event-created";
    public const string EventDeleted = "event-deleted";
    public const string EventSeatsReserved = "event-seats-reserved";

    // User события
    public const string UserCreated = "user-created";
    public const string UserDeleted = "user-deleted";
}
