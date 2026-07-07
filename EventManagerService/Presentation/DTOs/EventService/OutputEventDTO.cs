using EventManagerService.Domain.Models.DomainEvent;
using System.ComponentModel.DataAnnotations;

namespace EventManagerService.Presentation.DTOs.EventService
{
    public record OutputEventDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

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
