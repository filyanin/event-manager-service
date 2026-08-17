using System.ComponentModel.DataAnnotations;

namespace EventService.Application.DTOs
{
    public record InputEventDTO
    {
        [Required]
        [StringLength(1000, MinimumLength = 6)]
        public string Title { get; init; }
        public string? Description { get; init; }
        [Required]
        public DateTime? StartAt { get; init; }
        [Required]
        public DateTime? EndAt { get; init; }
        [Required]
        [Range(1, int.MaxValue)]
        public int? TotalSeat { get; init; }
    }
}
