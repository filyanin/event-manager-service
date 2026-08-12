namespace EventService.Infrastructure.DataAssets.Models
{
    /// <summary>
    /// Журнал обработанных событий BookingCancelled. Используется для идемпотентной обработки:
    /// при повторной доставке (или дублировании) сообщения с тем же BookingId места повторно не освобождаются.
    /// </summary>
    public class ProcessedBookingCancellation
    {
        public Guid BookingId { get; set; }

        public DateTime ProcessedAt { get; set; }
    }
}
