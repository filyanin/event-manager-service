using EventManagerService.Domain.Models.Event;
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

        public OutputEventDTO(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }

        public OutputEventDTO(Event _event)
        {
            Id = _event.Id;
            Title = _event.Title;
            Description = _event.Description;
            StartAt = _event.StartAt;
            EndAt = _event.EndAt;
        }
    }
}
