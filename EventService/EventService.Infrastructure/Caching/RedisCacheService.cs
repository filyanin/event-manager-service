using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using EventService.Application.Interfaces;

namespace EventService.Infrastructure.Caching;

/// <summary>
/// Реализация ICacheService поверх StackExchange.Redis.
/// Все ошибки взаимодействия с Redis логируются, но не пробрасываются наружу:
/// при недоступности кеша вызывающий код должен просто обратиться к базе данных.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var value = await db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>((string)value!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить значение из Redis по ключу {Key}. Кеш будет пропущен.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var serialized = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, serialized, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось записать значение в Redis по ключу {Key}. Кеш пропущен.", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось удалить ключ {Key} из Redis. Инвалидация кеша пропущена.", key);
        }
    }
}
