using EventManagerService.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EventManagerService.Presentation.DTOs
{
    public record OutputEventDTO
    {
        public Guid id;
        public string title;
        public string? description;
        public DateTime startAt;
        public DateTime endAt;

        public OutputEventDTO(Guid _id, string _title, DateTime _startAt, DateTime _endAt, string? description = null)
        {
            this.id = _id;
            this.title = _title;
            this.description = description;
            this.startAt = _startAt;
            this.endAt = _endAt;
        }

        public OutputEventDTO(Event _event)
        {
            this.id = _event.Id;
            this.title = _event.Title;
            this.description = _event.Description;
            this.startAt = _event.StartAt;
            this.endAt = _event.EndAt;
        }
    }
}
