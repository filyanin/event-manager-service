

using EventManagerService.Domain;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Infrastructure;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Tests
{
    public class DeleteEventTest
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly string _dbName;

        public DeleteEventTest()
        {
            _dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName));
            services.AddDomain();
            services.AddInfrastructure();
            _serviceProvider = services.BuildServiceProvider();

            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            eventService.AddEventAsync("Test event", DateTime.MinValue, DateTime.MaxValue, 100).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task DeleteEvent_CorrectId_SuccessDelete()
        {
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var ev = await eventService.AddEventAsync("Test event", DateTime.MinValue, DateTime.MaxValue, 100);

            await eventService.DeleteEventAsync(ev.Id);

            // Проверяем, что объект удалён из БД
            Assert.False(await context.Events.AnyAsync(e => e.Id == ev.Id));

        }
        [Fact]
        public async Task DeleteEvent_WrongId_KeyNotFoundException()
        {
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await Assert.ThrowsAsync<KeyNotFoundException>(() => eventService.DeleteEventAsync(Guid.NewGuid()));
        }

    }
}
