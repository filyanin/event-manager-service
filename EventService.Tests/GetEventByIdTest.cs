
using EventManagerService.Application;
using EventManagerService.Application.Interfaces;
using EventManagerService.Domain;
using EventManagerService.Infrastructure;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using EventManagerService.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Tests
{
    public class GetEventByIdTest
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly string _dbName;

        public GetEventByIdTest()
        {
            _dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName));

            // Создаем конфигурацию для тестов
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtSettings:Secret", "test-secret-key-at-least-32-characters-for-testing" },
                    { "JwtSettings:Issuer", "TestIssuer" },
                    { "JwtSettings:Audience", "TestAudience" },
                    { "JwtSettings:ExpirationMinutes", "60" }
                })
                .Build();

            services.AddApplication();  
            services.AddInfrastructure(configuration);
            _serviceProvider = services.BuildServiceProvider();

            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            eventService.AddEventAsync(new InputEventDTO
            {
                Title = "Test event",
                StartAt = DateTime.MinValue,
                EndAt = DateTime.MaxValue,
                TotalSeat = 100
            }).GetAwaiter().GetResult();
        }
        [Fact]
        public async Task GetEventById_CorrectId_SuccessGetEvent()
        {
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

            var ev = await eventService.AddEventAsync(new InputEventDTO
            {
                Title = "Event to update",
                StartAt = DateTime.MinValue,
                EndAt = DateTime.MaxValue,
                TotalSeat = 100
            });

            var anotherEvent = await eventService.GetEventByIdAsync(ev.Id);

            Assert.NotNull(anotherEvent);
            Assert.Equal(ev.Id, anotherEvent.Id);
        }
        [Fact]
        public async Task GetEventById_WrongId_KeyNotFoundException()
        {
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var ex = await Assert.ThrowsAsync<EventManagerService.Shared.Exceptions.AppException>(() => eventService.GetEventByIdAsync(Guid.NewGuid()));
            Assert.Equal(EventManagerService.Shared.ErrorCodes.ErrorCodes.NotFound, ex.ErrorCode);
        }

    }
}
