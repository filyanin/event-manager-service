using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models;

namespace EventManagerService.Infrastructure.DataAssets.Models
{
    public class Booking
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public Event Event { get; set; }

        public Booking() { }

        public DomainBooking ConvertToDomainBooking()
        {
            return new DomainBooking(Id, EventId, Status, CreatedAt, ProcessedAt);
        }

    }
}
