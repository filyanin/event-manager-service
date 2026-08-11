using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.Configuration;
using System.Text.Json;

namespace BookingService.Infrastructure.Kafka;

public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, string key, T message);
}

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IOptions<KafkaSettings> settings, ILogger<KafkaProducer> logger)
    {
        var kafkaSettings = settings.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            Acks = Acks.All,
            Retries = 3,
            MessageSendMaxRetries = 3,
            RequestTimeoutMs = 30000,
            LingerMs = 10
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetValueSerializer(Serializers.Utf8)
            .Build();
        _logger = logger;
    }

    public async Task PublishAsync<T>(string topic, string key, T message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });

            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = key,
                Value = json
            });

            _logger.LogInformation($"Event published to topic '{topic}' at partition {result.Partition.Value}, offset {result.Offset.Value}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error publishing message to Kafka topic '{topic}': {ex.Message}");
            throw;
        }
    }
}
