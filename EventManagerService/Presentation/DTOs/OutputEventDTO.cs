using System.ComponentModel.DataAnnotations;

namespace EventManagerService.Presentation.DTOs
{
    public record OutputEventDTO
    {
        Guid id;
        public required string title;
        public string? description;
        public DateTime? startAt;
        public DateTime? endAt;
    }
}
