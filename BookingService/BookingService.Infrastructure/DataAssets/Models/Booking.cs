using BookingService.Domain.Models;
using BookingService.Domain.Enum;

namespace BookingService.Infrastructure.DataAssets.Models
{
    public class Booking
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public Guid UserId { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public Booking() { }

        public DomainBooking ConvertToDomainBooking()
        {
            return new DomainBooking(Id, EventId, UserId, Status, CreatedAt, ProcessedAt);
        }
    }
}
