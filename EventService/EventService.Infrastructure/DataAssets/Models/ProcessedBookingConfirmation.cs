namespace EventService.Infrastructure.DataAssets.Models
{
    /// <summary>
    /// Журнал обработанных событий BookingConfirmed. Используется для идемпотентной обработки:
    /// при повторной доставке (или дублировании) сообщения с тем же BookingId места повторно не списываются.
    /// </summary>
    public class ProcessedBookingConfirmation
    {
        public Guid BookingId { get; set; }

        public DateTime ProcessedAt { get; set; }
    }
}
