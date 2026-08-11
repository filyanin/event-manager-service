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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingConfirmed consumer service starting");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IKafkaConsumer<BookingConfirmedEvent>>();
            var handler = scope.ServiceProvider.GetRequiredService<BookingConfirmedEventHandler>();

            await consumer.StartAsync(
                KafkaTopics.BookingConfirmed,
                handler.HandleAsync,
                stoppingToken
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("BookingConfirmed consumer service was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in BookingConfirmed consumer service: {ex.Message}. Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BookingConfirmed consumer service stopping");
        await base.StopAsync(cancellationToken);
    }
}
