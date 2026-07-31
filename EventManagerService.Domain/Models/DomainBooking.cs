using EventManagerService.Domain.Enum;

namespace EventManagerService.Domain.Models
{
    public class DomainBooking
    {
        public Guid Id { get; private set; }

        public Guid EventId { get; private set; }

        public BookingStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? ProcessedAt { get; private set; }

        // Конструктор не приватный, т.к. в нём нет логики
        public DomainBooking(Guid eventId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = BookingStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            ProcessedAt = null;
        }

        public DomainBooking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
        {
            if (processedAt != null && createdAt >= processedAt)
            {
                var ex = new Exceptions.GreaterThenValidationException(
                    EventManagerService.Shared.ErrorCodes.ErrorCodes.GreaterThanValidationError,
                    nameof(processedAt),
                    nameof(createdAt),
                    processedAt,
                    createdAt);
                throw ex;
            }

            Id = id;
            EventId = eventId;
            Status = status;
            CreatedAt = createdAt;
            ProcessedAt = processedAt;

        }


        public void SetBookingConfirmed(DateTime processedAt)
        {
            if (!Status.Equals(BookingStatus.Pending))
                throw new Exceptions.GreaterThenValidationException(EventManagerService.Shared.ErrorCodes.ErrorCodes.TryChangeCompletedBookingError);

            if (CreatedAt >= processedAt)
            {
                var ex = new Exceptions.GreaterThenValidationException(
                    EventManagerService.Shared.ErrorCodes.ErrorCodes.GreaterThanValidationError,
                    nameof(processedAt),
                    nameof(CreatedAt),
                    processedAt,
                    CreatedAt);
                throw ex;
            }

            Status = BookingStatus.Confirmed;
            ProcessedAt = processedAt;
        }
        public void SetBookingRejected(DateTime rejectedAt)
        {
            if (!Status.Equals(BookingStatus.Pending))
                throw new Exceptions.GreaterThenValidationException(EventManagerService.Shared.ErrorCodes.ErrorCodes.TryChangeCompletedBookingError);

            if (CreatedAt >= rejectedAt)
            {
                var ex = new Exceptions.GreaterThenValidationException(
                    EventManagerService.Shared.ErrorCodes.ErrorCodes.GreaterThanValidationError,
                    nameof(rejectedAt),
                    nameof(CreatedAt),
                    rejectedAt,
                    CreatedAt);
                throw ex;
            }

            Status = BookingStatus.Rejected;
            ProcessedAt = rejectedAt;
        }

    }
}
