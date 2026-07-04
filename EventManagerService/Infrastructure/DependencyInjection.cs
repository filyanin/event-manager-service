using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace EventManagerService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<BookingBackgroundService>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

    }
}
