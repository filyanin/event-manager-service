using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagerService.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDomain(this IServiceCollection services) 
        {
            services.AddScoped<IEventService, EventService>();

            services.AddScoped<IBookingService, BookingService>();

            return services;
        }

    }
}
