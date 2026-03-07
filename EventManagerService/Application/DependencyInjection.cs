using EventManagerService.Application.Interfaces;
using System.Runtime.CompilerServices;

namespace EventManagerService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAplication(this IServiceCollection services)
        {
            services.AddScoped<IQuerryMapper, QuerryMapper>();

            return services; 
        }
    }
}
