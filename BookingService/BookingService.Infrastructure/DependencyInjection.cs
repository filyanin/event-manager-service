using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingService.Application.Interfaces;
using BookingService.Infrastructure.DataAssets;
using BookingService.Infrastructure.Repositories;
using BookingService.Infrastructure.Kafka;
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

        return services;
    }
}

