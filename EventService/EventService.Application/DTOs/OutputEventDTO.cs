using EventService.Domain.Models;

namespace EventService.Application.DTOs
{
    public record OutputEventDTO
    {
        public Guid Id { get; init; }
        public string Title { get; init; }
        public string? Description { get; init; }
        public DateTime StartAt { get; init; }
        public DateTime EndAt { get; init; }
        public int TotalSeats { get; init; }
        public int AvailableSeats { get; init; }

        public OutputEventDTO(Guid id, string title, DateTime startAt, DateTime endAt,int totalSeats,int availableSeats, string? description = null)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = availableSeats;
        }

        public OutputEventDTO(DomainEvent _event)
        {
            Id = _event.Id;
            Title = _event.Title;
            Description = _event.Description;
            StartAt = _event.StartAt;
            EndAt = _event.EndAt;
            TotalSeats= _event.TotalSeats;
            AvailableSeats= _event.AvailableSeats;
        }
    }
}
