using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain;
using Microsoft.Extensions.DependencyInjection;
using EventManagerService.Domain.Models.DomainEvent;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
            services.AddDomain();
            _serviceProvider = services.BuildServiceProvider();

            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            eventService.AddEventAsync("Test event", DateTime.MinValue, DateTime.MaxValue, 100).GetAwaiter().GetResult();
        }
        [Fact]
        public async Task GetEventById_CorrectId_SuccessGetEvent()
        {
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

            var ev = await eventService.AddEventAsync("Event to update", DateTime.MinValue, DateTime.MaxValue, 100);

            var anotherEvent = await eventService.GetEventByIdAsync(ev.Id);

            Assert.NotNull(anotherEvent);
            Assert.Equal(ev.Id, anotherEvent.Id);
        }
        [Fact]
        public async Task GetEventById_WrongId_KeyNotFoundException()
        {
            using var scope = _serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await Assert.ThrowsAsync<KeyNotFoundException>(() => eventService.GetEventByIdAsync(Guid.NewGuid()));
        }

    }
}
