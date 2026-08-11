using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.Configuration;
using System.Text.Json;

namespace EventService.Infrastructure.Kafka;

public interface IKafkaConsumer<T>
{
    Task StartAsync(string topic, Func<T, Task> handler, CancellationToken cancellationToken);
}

public class KafkaConsumer<T> : IKafkaConsumer<T>, IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumer<T>> _logger;

    public KafkaConsumer(IOptions<KafkaSettings> settings, ILogger<KafkaConsumer<T>> logger)
    {
        var kafkaSettings = settings.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = kafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SessionTimeoutMs = kafkaSettings.SessionTimeoutMs,
            HeartbeatIntervalMs = kafkaSettings.HeartbeatIntervalMs,
            EnableAutoCommit = false,
            EnablePartitionEof = false
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetValueDeserializer(Deserializers.Utf8)
            .SetLogHandler((consumer, log) =>
            {
                if (log.Level >= SyslogLevel.Warning)
                {
                    _logger.LogWarning($"Kafka log: {log.Message}");
                }
            })
            .SetErrorHandler((consumer, error) =>
            {
                if (!error.IsFatal)
                {
                    _logger.LogWarning($"Kafka error: {error.Reason}");
                }
                else
                {
                    _logger.LogError($"Kafka fatal error: {error.Reason}");
                }
            })
            .Build();

        _logger = logger;
    }

    public async Task StartAsync(string topic, Func<T, Task> handler, CancellationToken cancellationToken)
    {
        _consumer.Subscribe(topic);
        _logger.LogInformation($"Kafka consumer subscribed to topic '{topic}' with group '{_consumer.MemberId}'");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(TimeSpan.FromSeconds(5));

                    if (result == null)
                        continue;

                    _logger.LogInformation($"Received message from topic '{topic}': {result.Message.Value}");

                    try
                    {
                        var message = JsonSerializer.Deserialize<T>(result.Message.Value, new JsonSerializerOptions 
                        { 
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                        });

                        if (message != null)
                        {
                            await handler(message);
                            _consumer.Commit(result);
                            _logger.LogInformation($"Message processed and committed from topic '{topic}'");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Error deserializing message from topic '{topic}': {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing message from topic '{topic}': {ex.Message}");
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError($"Error consuming from Kafka: {ex.Error.Reason}");
                }
            }
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation($"Kafka consumer for topic '{topic}' closed");
        }
    }

    public void Dispose()
    {
        _consumer?.Dispose();
    }
}
