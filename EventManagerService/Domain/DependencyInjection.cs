using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Services.EventService;

namespace EventManagerService.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDomain(this IServiceCollection services) 
        {
            services.AddSingleton<IEventService, EventService>();

            return services;
        }

    }
}
