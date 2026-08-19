using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events.Booking;
using Shared.Contracts.Topics;
using BookingService.Infrastructure.Kafka;
using BookingService.Infrastructure.Kafka.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure.Kafka.HostedServices;

public class BookingSeatsRejectedConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingSeatsRejectedConsumerHostedService> _logger;

    public BookingSeatsRejectedConsumerHostedService(IServiceProvider serviceProvider, ILogger<BookingSeatsRejectedConsumerHostedService> logger)
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
        _logger.LogInformation("BookingSeatsRejected consumer service starting");

        try
        {
            using var consumerScope = _serviceProvider.CreateScope();
            var consumer = consumerScope.ServiceProvider.GetRequiredService<IKafkaConsumer<BookingSeatsRejectedEvent>>();

            await consumer.StartAsync(
                KafkaTopics.BookingSeatsRejected,
                HandleMessageAsync,
                stoppingToken
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("BookingSeatsRejected consumer service was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BookingSeatsRejected consumer service: {Message}", ex.Message);
        }
    }

    private async Task HandleMessageAsync(BookingSeatsRejectedEvent @event)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<BookingSeatsRejectedEventHandler>();
        await handler.HandleAsync(@event);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BookingSeatsRejected consumer service stopping");
        await base.StopAsync(cancellationToken);
    }
}
