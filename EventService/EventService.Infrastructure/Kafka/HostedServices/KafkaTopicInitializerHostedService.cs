using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.Configuration;
using Shared.Contracts.Topics;

namespace EventService.Infrastructure.Kafka.HostedServices;

/// <summary>
/// Гарантирует наличие топика Kafka, на который подписывается EventService, до старта подписчика.
/// Работает как обычный IHostedService (не BackgroundService): выполняется один раз при старте
/// приложения и завершается, не блокируя запуск остальных сервисов при ошибке.
/// </summary>
public class KafkaTopicInitializerHostedService : IHostedService
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<KafkaTopicInitializerHostedService> _logger;

    private static readonly string[] TopicsToEnsure =
    {
        KafkaTopics.BookingConfirmed
    };

    public KafkaTopicInitializerHostedService(IOptions<KafkaSettings> settings, ILogger<KafkaTopicInitializerHostedService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = _settings.BootstrapServers
            };

            using var adminClient = new AdminClientBuilder(adminConfig).Build();

            var topicSpecifications = TopicsToEnsure
                .Select(topic => new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                })
                .ToList();

            try
            {
                await adminClient.CreateTopicsAsync(topicSpecifications);

                foreach (var topic in TopicsToEnsure)
                {
                    _logger.LogInformation("Kafka topic '{Topic}' created", topic);
                }
            }
            catch (CreateTopicsException ex)
            {
                foreach (var result in ex.Results)
                {
                    if (result.Error.Code == ErrorCode.TopicAlreadyExists)
                    {
                        _logger.LogInformation("Kafka topic '{Topic}' already exists, skipping creation", result.Topic);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create Kafka topic '{Topic}': {Reason}", result.Topic, result.Error.Reason);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Не валим запуск приложения, если топик создать не удалось (например, Kafka ещё не поднялась).
            // Подписчик всё равно попытается подписаться на топик самостоятельно.
            _logger.LogWarning(ex, "Failed to ensure Kafka topics exist. The consumer will attempt to use them anyway.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
