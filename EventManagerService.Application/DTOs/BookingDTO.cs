using EventManagerService.Domain.Enum;

namespace EventManagerService.Application.DTOs
{
    public class BookingDTO
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public BookingStatus Status {  get; set; }

    }
}
