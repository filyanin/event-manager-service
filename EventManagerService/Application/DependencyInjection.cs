using EventManagerService.Application.Interfaces.BookingService;
using EventManagerService.Application.Interfaces.EventService;
using EventManagerService.Application.Services;
using EventManagerService.Application.Services.EventService;

namespace EventManagerService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventQueryMapper, EventQueryMapper>();
            services.AddScoped<IBookingQueryMapper, BookingQueryMapper>();

            return services; 
        }
    }
}
