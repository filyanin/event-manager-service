using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Services;
using UserService.Application.Interfaces;

namespace UserService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
