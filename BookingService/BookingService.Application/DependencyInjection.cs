using Microsoft.Extensions.DependencyInjection;
using BookingService.Application.Interfaces;
using BookingService.Application.Services;

namespace BookingService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService.Application.Services.BookingService>();

        return services;
    }
}
