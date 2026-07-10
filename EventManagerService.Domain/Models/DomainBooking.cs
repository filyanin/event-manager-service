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
            Id = id;
            EventId = eventId;
            Status = status;
            CreatedAt = createdAt;
            ProcessedAt = processedAt;

        }


        public void SetBookingConfirmed(DateTime processedAt)
        {
            if (!Status.Equals(BookingStatus.Pending))
                throw new InvalidOperationException("TryChangeCompletedBookingError");

            if (CreatedAt >= processedAt)
            {
                var ex = new Exceptions.DomainValidationException("GreaterThanValidationError");
                ex.Data["firstParamName"] = nameof(processedAt);
                ex.Data["firstParamValue"] = processedAt;
                ex.Data["secondParamName"] = nameof(CreatedAt);
                ex.Data["secondParamValue"] = CreatedAt;
                throw ex;
            }

            Status = BookingStatus.Confirmed;
            ProcessedAt = processedAt;
        }
        public void SetBookingRejected(DateTime rejectedAt)
        {
            if (!Status.Equals(BookingStatus.Pending))
                throw new InvalidOperationException("TryChangeCompletedBookingError");

            if (CreatedAt >= rejectedAt)
            {
                var ex = new Exceptions.DomainValidationException("GreaterThanValidationError");
                ex.Data["firstParamName"] = nameof(rejectedAt);
                ex.Data["firstParamValue"] = rejectedAt;
                ex.Data["secondParamName"] = nameof(CreatedAt);
                ex.Data["secondParamValue"] = CreatedAt;
                throw ex;
            }

            Status = BookingStatus.Rejected;
            ProcessedAt = rejectedAt;
        }
    }
}
