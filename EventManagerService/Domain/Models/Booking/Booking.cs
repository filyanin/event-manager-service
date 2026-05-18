using EventManagerService.Domain.Enum;
using EventManagerService.Properties;
using System.Resources;

namespace EventManagerService.Domain.Models.Booking
{
    public class Booking
    {
        public Guid Id { get; private set; }

        public Guid EventId { get; private set; }

        public BookingStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? ProcessedAt { get; private set; }

        // Конструктор не приватный, т.к. В нём нет логики 
        public Booking(Guid eventId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = BookingStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            ProcessedAt = null;
        }

        //Логика подтверждения вынесена отдельно на случай изменения поведения в будущем
        public void SetBookingConfirmed(DateTime processedAt)
        {
            if (!Status.Equals(BookingStatus.Pending))
                throw new InvalidOperationException(new ResourceManager(typeof(ErrorMessages)).GetString("TryChangeCompletedBookingError"));

            if (CreatedAt >= processedAt)
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"), nameof(processedAt), nameof(CreatedAt)));

            Status = BookingStatus.Confirmed;
            ProcessedAt = processedAt;
        }

        //Логика отмены вынесена отдельно на случай изменения поведения в будущем
        public void SetBookingRejected(DateTime rejectedAt)
        {
            if (!Status.Equals(BookingStatus.Pending))
                throw new InvalidOperationException(new ResourceManager(typeof(ErrorMessages)).GetString("TryChangeCompletedBookingError"));

            if (CreatedAt >= rejectedAt)
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"), nameof(rejectedAt), nameof(CreatedAt)));

            Status = BookingStatus.Rejected;
            ProcessedAt = rejectedAt;
        }
    }
}
