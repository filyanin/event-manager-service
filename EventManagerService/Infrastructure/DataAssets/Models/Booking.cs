using EventManagerService.Domain.Enum;

namespace EventManagerService.Infrastructure.DataAssets.Models
{
    public class Booking
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public Models.Event Event { get; set; }

        public Booking() { }

        public Domain.Models.DomainBooking.DomainBooking ConvertTo()
        {
            // Не конвертируем связанный Event здесь, чтобы избежать рекурсии при преобразовании
            return new Domain.Models.DomainBooking.DomainBooking(Id, EventId, Status, CreatedAt, ProcessedAt, null);
        }

    }
}
