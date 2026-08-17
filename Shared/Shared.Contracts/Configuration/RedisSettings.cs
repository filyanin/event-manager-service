namespace Shared.Contracts.Configuration;

public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public int EventTtlSeconds { get; set; } = 300;
    public int TopEventsTtlSeconds { get; set; } = 60;
    public string EventKeyPrefix { get; set; } = "event:";
    public string TopEventsKey { get; set; } = "events:top10";
}
