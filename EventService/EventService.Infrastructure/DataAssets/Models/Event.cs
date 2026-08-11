using EventService.Domain.Models;

namespace EventService.Infrastructure.DataAssets.Models
{
    public class Event
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public byte[]? Timestamp { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        public Guid CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Event() { }

        public DomainEvent ConvertToDomainEvent()
        {
            var domainEvent = DomainEvent.Create(Id, Title, StartAt, EndAt, TotalSeats, CreatedByUserId, Description);

            return domainEvent;
        }
    }
}
