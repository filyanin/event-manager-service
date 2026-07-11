using EventManagerService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagerService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Репозитории зарегистрированы здесь, в Infrastructure, чтобы централизовать работу с хранилищем
            services.AddScoped<IEventRepository, Repositories.EventRepository>();
            services.AddScoped<IBookingRepository, Repositories.BookingRepository>();



            return services;
        }

    }
}
