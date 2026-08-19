using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingService.Application.Interfaces;
using BookingService.Infrastructure.DataAssets;
using BookingService.Infrastructure.Repositories;
using BookingService.Infrastructure.Kafka;
using BookingService.Infrastructure.Kafka.Handlers;
using BookingService.Infrastructure.Kafka.HostedServices;
using Shared.Contracts.Configuration;

namespace BookingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IBookingRepository, BookingRepository>();

        // Kafka configuration
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddSingleton<IKafkaProducer, KafkaProducer>();
        services.AddScoped(typeof(IKafkaConsumer<>), typeof(KafkaConsumer<>));
        services.AddScoped<BookingSeatsRejectedEventHandler>();
        // Порядок регистрации важен: инициализатор топика должен успеть отработать до старта подписчика.
        services.AddHostedService<KafkaTopicInitializerHostedService>();
        services.AddHostedService<BookingSeatsRejectedConsumerHostedService>();

        return services;
    }
}

