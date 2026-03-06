using System.ComponentModel.DataAnnotations;

namespace EventManagerService.Presentation.DTOs
{
    public record EventDTO
    {
        [Required]
        public required string title;
        public string? description;
        [Required]
        public DateTime? startAt;
        [Required]
        public DateTime? endAt;
    }
}
