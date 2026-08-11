using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BookingService.Application.Interfaces;
using BookingService.Infrastructure.DataAssets;
using BookingService.Infrastructure.Repositories;

namespace BookingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }
}
