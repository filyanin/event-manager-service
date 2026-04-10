using EventManagerService.Presentation.Validators;
using System.ComponentModel.DataAnnotations;

namespace EventManagerService.Presentation.DTOs.Event
{
    public record InputEventDTO
    {
        [Required]
        [StringLength(1000, MinimumLength = 6)]
        public string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public DateTime? StartAt { get; set; }
        [Required]
        [GreaterThan(nameof(StartAt))]
        public DateTime? EndAt { get; set; }
    }


}
