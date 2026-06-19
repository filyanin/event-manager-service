using EventManagerService.Presentation.Validators;
using System.ComponentModel.DataAnnotations;

namespace EventManagerService.Presentation.DTOs.EventService
{
    public record InputEventDTO
    {
        [Required]
        [StringLength(1000, MinimumLength = 6)]
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
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
