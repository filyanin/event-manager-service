using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventManagerService.Infrastructure;
using EventManagerService.Domain;
using EventManagerService.Application.Interfaces;
using EventManagerService.Application;
using EventManagerService.Application.DTOs;
using EventManagerService.Domain.Exceptions;


namespace EventService.Tests
{
    public class AddEventTest
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly string _dbName;

        public AddEventTest()
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

            services.AddInfrastructure(configuration);
            services.AddApplication();
            _serviceProvider = services.BuildServiceProvider();
        }

        [Theory]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z", "Test description")]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-01T11:24:15.444Z", "Test description")]
        [InlineData("Test event", "2025-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z", "Test description")]
        public async Task AddEvent_CorrectInputData_SuccessCreate(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var dto = new InputEventDTO { Title = title, StartAt = startAt, EndAt = endAt, TotalSeat = 100, Description = description };
            var ev = await eventService.AddEventAsync(dto);

            Assert.NotNull(ev);
            Assert.Equal(title, ev.Title);
            Assert.Equal(startAt, ev.StartAt);
            Assert.Equal(endAt, ev.EndAt);
            Assert.Equal(description, ev.Description);
            Assert.True(await context.Events.AnyAsync(e => e.Id == ev.Id));
        }


        [Theory]
        [InlineData("", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("12", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("TTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT",
            "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        public async Task AddEvent_WrongTitleString_ArgumentException(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var dto = new InputEventDTO { Title = title, StartAt = startAt, EndAt = endAt, TotalSeat = 100, Description = description };
            var ex = await Assert.ThrowsAnyAsync<Exception>(() => eventService.AddEventAsync(dto));
            Assert.True(ex is DomainValidationException || ex is ArgumentException);
        }


        [Theory]
        [InlineData("Test event", "2026-04-02T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:15.444Z", "2026-04-01T11:24:14.444Z")]
        [InlineData("Test event", "2027-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z")]
        public async Task AddEvent_StartDateGreaterThenEndDate_ArgumentException(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var dto = new InputEventDTO { Title = title, StartAt = startAt, EndAt = endAt, TotalSeat = 100, Description = description };
            var ex = await Assert.ThrowsAnyAsync<Exception>(() => eventService.AddEventAsync(dto));
            Assert.True(ex is DomainValidationException || ex is ArgumentException);
        }
    }
}
