using BookingService.Domain.Enum;

namespace BookingService.Domain.Models
{
    public class DomainBooking
    {
        public Guid Id { get; private set; }

        public Guid EventId { get; private set; }

        public Guid UserId { get; private set; }

        public BookingStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? ProcessedAt { get; private set; }

        public DomainBooking(Guid eventId, Guid userId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            UserId = userId;
            Status = BookingStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            ProcessedAt = null;
        }

        public DomainBooking(Guid id, Guid eventId, Guid userId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
        {
            if (processedAt != null && createdAt >= processedAt)
            {
                throw new System.ArgumentException("processedAt must be greater than createdAt");
            }

            Id = id;
            EventId = eventId;
            UserId = userId;
            Status = status;
            CreatedAt = createdAt;
            ProcessedAt = processedAt;
        }

        public void SetBookingConfirmed(DateTime processedAt)
        {
            if (!Status.Equals(BookingStatus.Pending))
                throw new System.InvalidOperationException("Cannot confirm non-pending booking");

            if (CreatedAt >= processedAt)
                throw new System.ArgumentException("processedAt must be greater than CreatedAt");

            Status = BookingStatus.Confirmed;
            ProcessedAt = processedAt;
        }

        public void SetBookingRejected(DateTime rejectedAt)
        {
            if (!Status.Equals(BookingStatus.Pending))
                throw new System.InvalidOperationException("Cannot reject non-pending booking");

            if (CreatedAt >= rejectedAt)
                throw new System.ArgumentException("rejectedAt must be greater than CreatedAt");

            Status = BookingStatus.Rejected;
            ProcessedAt = rejectedAt;
        }

        public void SetBookingCancelled(DateTime cancelledAt)
        {
            if (!Status.Equals(BookingStatus.Pending) && !Status.Equals(BookingStatus.Confirmed))
                throw new System.InvalidOperationException("Cannot cancel booking in its current state");

            if (CreatedAt >= cancelledAt)
                throw new System.ArgumentException("cancelledAt must be greater than CreatedAt");

            Status = BookingStatus.Cancelled;
            ProcessedAt = cancelledAt;
        }
    }
}
