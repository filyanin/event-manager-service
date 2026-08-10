using EventManagerService.Application.Interfaces;
using EventManagerService.Infrastructure.Repositories;
using EventManagerService.Infrastructure.Security;
using EventManagerService.Infrastructure.Security.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagerService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Репозитории зарегистрированы здесь, в Infrastructure, чтобы централизовать работу с хранилищем
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Сервис для хеширования паролей
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            // Регистрация конфигурации JWT
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            // Сервис для генерации JWT-токенов
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }

    }
}


