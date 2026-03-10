using EventManagerService.Domain.Interfaces;

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
