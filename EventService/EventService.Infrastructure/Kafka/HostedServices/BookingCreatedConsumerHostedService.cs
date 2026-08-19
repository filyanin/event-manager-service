using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Booking;
using Shared.Contracts.Topics;
using EventService.Infrastructure.Kafka;
using EventService.Infrastructure.Kafka.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Infrastructure.Kafka.HostedServices;

public class BookingCreatedConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingCreatedConsumerHostedService> _logger;

    public BookingCreatedConsumerHostedService(IServiceProvider serviceProvider, ILogger<BookingCreatedConsumerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => RunConsumerLoopAsync(stoppingToken), stoppingToken);
    }

    private async Task RunConsumerLoopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingCreated consumer service starting");

        try
        {
            using var consumerScope = _serviceProvider.CreateScope();
            var consumer = consumerScope.ServiceProvider.GetRequiredService<IKafkaConsumer<BookingCreatedEvent>>();

            await consumer.StartAsync(
                KafkaTopics.BookingCreated,
                HandleMessageAsync,
                stoppingToken
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("BookingCreated consumer service was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BookingCreated consumer service: {Message}", ex.Message);
        }
    }

    private async Task HandleMessageAsync(BookingCreatedEvent @event)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<BookingCreatedEventHandler>();
        await handler.HandleAsync(@event);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BookingCreated consumer service stopping");
        await base.StopAsync(cancellationToken);
    }
}
