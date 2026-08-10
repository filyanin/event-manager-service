using BookingService.Domain.Enum;

namespace BookingService.Application.DTOs
{
    public class BookingDTO
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public BookingStatus Status { get; set; }
    }
}
