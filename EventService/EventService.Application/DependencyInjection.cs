using Microsoft.Extensions.DependencyInjection;
using EventService.Application.Interfaces;
using EventService.Application.Services;

namespace EventService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService.Application.Services.EventService>();

        return services;
    }
}
