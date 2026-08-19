namespace EventService.Application.Interfaces
{
    /// <summary>
    /// Абстракция над кешем. Реализация не должна пробрасывать исключения наружу:
    /// при недоступности кеша операции должны деградировать (промах кеша / no-op),
    /// а ошибка — логироваться на уровне реализации.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Возвращает значение по ключу, либо default(T), если значения нет или кеш недоступен.
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Сохраняет значение по ключу с временем жизни. При недоступности кеша операция молча пропускается.
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan ttl);

        /// <summary>
        /// Удаляет значение по ключу. При недоступности кеша операция молча пропускается.
        /// </summary>
        Task RemoveAsync(string key);
    }
}
