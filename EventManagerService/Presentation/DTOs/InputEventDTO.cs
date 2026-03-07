using System.ComponentModel.DataAnnotations;

namespace EventManagerService.Presentation.DTOs
{
    public record InputEventDTO
    {
        [Required]
        [StringLength(1000,MinimumLength = 6)]
        public required string title;
        public string? description;
        [Required]
        public DateTime? startAt;
        [Required]
        public DateTime? endAt;
    }


}
