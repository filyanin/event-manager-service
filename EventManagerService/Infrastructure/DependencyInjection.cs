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

            // Регистрация DbContext
            services.AddDbContext<DataAssets.AppDbContext>(options =>
            {
                // Пустая конфигурация - предполагается настройка в Program.cs или appsettings
            });

            // Репозитории зарегистрированы в Domain.DependencyInjection как Scoped, но дублируем для надежности
            services.AddScoped<Domain.Interfaces.Repositories.IEventRepository, Repositories.EventRepository>();
            services.AddScoped<Domain.Interfaces.Repositories.IBookingRepository, Repositories.BookingRepository>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

    }
}
