using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Services.BookingService;
using EventManagerService.Domain.Services.EventService;

namespace EventManagerService.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDomain(this IServiceCollection services) 
        {
            services.AddScoped<IEventService, EventService>();

            services.AddScoped<IBookingService, BookingService>();

            // AppDbContext is registered in Infrastructure as scoped; domain services depend on it and must be scoped as well.

            return services;
        }

    }
}
