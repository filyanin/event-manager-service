using EventManagerService.Infrastructure.DataAssets;
using EventManagerService.Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventManagerService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddHostedService<BookingBackgroundService>();

            // Репозитории зарегистрированы здесь, в Infrastructure, чтобы централизовать работу с хранилищем
            services.AddScoped<IEventRepository, Repositories.EventRepository>();
            services.AddScoped<IBookingRepository, Repositories.BookingRepository>();



            return services;
        }

    }
}
