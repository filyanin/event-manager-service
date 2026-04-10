using EventManagerService.Application.Interfaces.EventService;
using EventManagerService.Application.Services.EventService;
using System.Runtime.CompilerServices;

namespace EventManagerService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IQueryMapper, QueryMapper>();

            return services; 
        }
    }
}
