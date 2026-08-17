namespace Shared.Contracts.Configuration;

/// <summary>
/// Централизованное место для формирования и хранения ключей кеша,
/// чтобы не разбрасывать строковые константы/шаблоны по коду.
/// </summary>
public static class CacheKeys
{
    public static string Event(string prefix, Guid id) => $"{prefix}{id}";

    public const string TopEventsDefault = "events:top10";
}
