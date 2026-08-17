using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventService.Application.Interfaces;
using EventService.Infrastructure.DataAssets;
using EventService.Infrastructure.Repositories;
using EventService.Infrastructure.Kafka;
using EventService.Infrastructure.Kafka.Handlers;
using EventService.Infrastructure.Kafka.HostedServices;
using Shared.Contracts.Configuration;
using Shared.Contracts.Events.Booking;

namespace EventService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IEventRepository, EventRepository>();

        // Kafka configuration
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddScoped(typeof(IKafkaConsumer<>), typeof(KafkaConsumer<>));
        services.AddScoped<BookingConfirmedEventHandler>();
        services.AddScoped<BookingCancelledEventHandler>();
        // Порядок регистрации важен: инициализатор топика должен успеть отработать до старта подписчика.
        services.AddHostedService<KafkaTopicInitializerHostedService>();
        services.AddHostedService<BookingConfirmedConsumerHostedService>();
        services.AddHostedService<BookingCancelledConsumerHostedService>();

        return services;
    }
}

