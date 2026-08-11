namespace Shared.Contracts.Configuration;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "default-group";
    public int SessionTimeoutMs { get; set; } = 30000;
    public int HeartbeatIntervalMs { get; set; } = 10000;
}
