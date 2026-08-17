using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Booking;
using Shared.Contracts.Topics;
using EventService.Infrastructure.Kafka;
using EventService.Infrastructure.Kafka.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Infrastructure.Kafka.HostedServices;

public class BookingConfirmedConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingConfirmedConsumerHostedService> _logger;

    public BookingConfirmedConsumerHostedService(IServiceProvider serviceProvider, ILogger<BookingConfirmedConsumerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Consume() у клиента Kafka — блокирующий вызов, поэтому весь цикл потребления
        // выполняется в отдельном потоке пула, чтобы не удерживать поток, на котором
        // BackgroundService.StartAsync запускает ExecuteAsync.
        return Task.Run(() => RunConsumerLoopAsync(stoppingToken), stoppingToken);
    }

    private async Task RunConsumerLoopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingConfirmed consumer service starting");

        try
        {
            // Сам консьюмер — обёртка над клиентом Kafka и не зависит от БД,
            // поэтому резолвим его один раз на время жизни фонового сервиса.
            using var consumerScope = _serviceProvider.CreateScope();
            var consumer = consumerScope.ServiceProvider.GetRequiredService<IKafkaConsumer<BookingConfirmedEvent>>();

            await consumer.StartAsync(
                KafkaTopics.BookingConfirmed,
                HandleMessageAsync,
                stoppingToken
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("BookingConfirmed consumer service was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BookingConfirmed consumer service: {Message}", ex.Message);
        }
    }

    private async Task HandleMessageAsync(BookingConfirmedEvent @event)
    {
        // BackgroundService — singleton, а репозиторий и DbContext — scoped,
        // поэтому на каждое обрабатываемое сообщение создаём собственный scope.
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<BookingConfirmedEventHandler>();
        await handler.HandleAsync(@event);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BookingConfirmed consumer service stopping");
        await base.StopAsync(cancellationToken);
    }
}
