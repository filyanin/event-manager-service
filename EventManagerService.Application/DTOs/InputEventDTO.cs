using EventManagerService.Application.Validators;
using System.ComponentModel.DataAnnotations;

namespace EventManagerService.Application.DTOs
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
        [Required]
        [Range(1,int.MaxValue)]
        public int? TotalSeat { get; set; }
    }


}
