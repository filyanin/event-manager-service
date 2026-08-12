using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Booking;
using Shared.Contracts.Topics;
using EventService.Infrastructure.Kafka;
using EventService.Infrastructure.Kafka.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Infrastructure.Kafka.HostedServices;

public class BookingCancelledConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingCancelledConsumerHostedService> _logger;

    public BookingCancelledConsumerHostedService(IServiceProvider serviceProvider, ILogger<BookingCancelledConsumerHostedService> logger)
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
        _logger.LogInformation("BookingCancelled consumer service starting");

        try
        {
            // Сам консьюмер — обёртка над клиентом Kafka и не зависит от БД,
            // поэтому резолвим его один раз на время жизни фонового сервиса.
            using var consumerScope = _serviceProvider.CreateScope();
            var consumer = consumerScope.ServiceProvider.GetRequiredService<IKafkaConsumer<BookingCancelledEvent>>();

            await consumer.StartAsync(
                KafkaTopics.BookingCancelled,
                HandleMessageAsync,
                stoppingToken
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("BookingCancelled consumer service was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BookingCancelled consumer service: {Message}", ex.Message);
        }
    }

    private async Task HandleMessageAsync(BookingCancelledEvent @event)
    {
        // BackgroundService — singleton, а репозиторий и DbContext — scoped,
        // поэтому на каждое обрабатываемое сообщение создаём собственный scope.
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<BookingCancelledEventHandler>();
        await handler.HandleAsync(@event);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BookingCancelled consumer service stopping");
        await base.StopAsync(cancellationToken);
    }
}
