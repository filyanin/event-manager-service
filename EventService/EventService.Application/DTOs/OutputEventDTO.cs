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
        public Guid CreatedByUserId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }

        public OutputEventDTO(Guid id, string title, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats, Guid createdByUserId, DateTime createdAt, string? description = null, DateTime? updatedAt = null)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = availableSeats;
            CreatedByUserId = createdByUserId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public OutputEventDTO(DomainEvent _event)
        {
            Id = _event.Id;
            Title = _event.Title;
            Description = _event.Description;
            StartAt = _event.StartAt;
            EndAt = _event.EndAt;
            TotalSeats = _event.TotalSeats;
            AvailableSeats = _event.AvailableSeats;
            CreatedByUserId = _event.CreatedByUserId;
            CreatedAt = _event.CreatedAt;
            UpdatedAt = _event.UpdatedAt;
        }
    }
}
