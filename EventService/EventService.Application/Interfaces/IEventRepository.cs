using EventService.Domain.Filters;
using EventService.Domain.Models;
using EventService.Domain.ValueObjects;

namespace EventService.Application.Interfaces
{
    public interface IEventRepository
    {
        public Task<(IList<DomainEvent> Items, int Total)> GetAllAsync(EventsFilters filters, Paginations paginations);

        public Task<DomainEvent> AddAsync(DomainEvent ev);

        public Task DeleteAsync(Guid id);

        public Task<DomainEvent> GetByIdAsync(Guid id);

        /// <summary>
        /// Возвращает топ событий, отсортированных по убыванию процента проданных мест:
        /// (TotalSeats - AvailableSeats) / TotalSeats.
        /// </summary>
        public Task<IList<DomainEvent>> GetTopEventsAsync(int count);

        public Task UpdateAsync(DomainEvent domainEvent);

        public Task<bool> ExistsAsync(Guid id);

        public Task<bool> TryReserveSeatsAsync(DomainEvent @event);

        /// <summary>
        /// Идемпотентно резервирует места для события в рамках обработки BookingConfirmed.
        /// Если сообщение с данным bookingId уже было обработано ранее, места повторно не списываются.
        /// Списание выполняется атомарно относительно актуального состояния события в БД (а не
        /// относительно значения, прочитанного до начала обработки), чтобы конкурентные подтверждения
        /// не перезаписывали друг друга.
        /// </summary>
        /// <returns>true, если места были зарезервированы; false, если bookingId уже был обработан ранее (дубликат)</returns>
        public Task<bool> TryReserveSeatsIdempotentAsync(Guid bookingId, Guid eventId, int seatsToReserve);

        public Task<bool> ReleaseSeatsAsync(DomainEvent @event);

        /// <summary>
        /// Идемпотентно освобождает места для события в рамках обработки BookingCancelled.
        /// Если сообщение с данным bookingId уже было обработано ранее, места повторно не возвращаются.
        /// </summary>
        /// <returns>true, если места были освобождены; false, если bookingId уже был обработан ранее (дубликат)</returns>
        public Task<bool> ReleaseSeatsIdempotentAsync(Guid bookingId, Guid eventId, int seatsToRelease);
    }
}
