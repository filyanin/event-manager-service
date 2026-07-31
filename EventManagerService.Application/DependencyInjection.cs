using EventManagerService.Application.Interfaces;
using EventManagerService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagerService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();

            services.AddHostedService<BookingBackgroundService>();

            return services;

        }
    }
}
