
using EventManagerService.Domain.Models;

namespace EventManagerService.Infrastructure.DataAssets.Models
{
    public class Event
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public byte[]? Timestamp { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public Event() { }

        public DomainEvent ConvertToDomainEvent()
        {
            var domainEvent = DomainEvent.Create(Id, Title, StartAt, EndAt, TotalSeats, AvailableSeats, Description);

            return domainEvent;
        }
    }

}
