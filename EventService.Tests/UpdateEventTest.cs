using EventManagerService.Domain.Interfaces.EventService;
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
    public  class UpdateEventTest
    {
        public IEventService eventService;
        public AppDbContext _context;

        public UpdateEventTest()
        {
            var options = new DbContextOptionsBuilder<EventManagerService.Infrastructure.DataAssets.AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new EventManagerService.Infrastructure.DataAssets.AppDbContext(options);
            var eventRepo = new EventManagerService.Infrastructure.Repositories.EventRepository(_context);
            eventService = new EventManagerService.Domain.Services.EventService.EventService(eventRepo);
            eventService.AddEventAsync("Test event", DateTime.MinValue, DateTime.MaxValue, 100).GetAwaiter().GetResult();
        }

        [Theory]
        [InlineData("New Good event", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("New Good event", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z", "Test description")]
        [InlineData("New Good event", "2026-04-01T11:24:14.444Z", "2026-04-01T11:24:15.444Z", "Test description")]
        [InlineData("New Good event", "2025-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z", "Test description")]
        public async Task UpdateEvent_CorrectInputData_SuccessUpdateEvent(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var ev = await eventService.AddEventAsync("Event to update",DateTime.MinValue, DateTime.MaxValue, 100);

            await eventService.UpdateEventAsync(ev.Id, title, startAt, endAt, description);

            var updated = await eventService.GetEventByIdAsync(ev.Id);

            Assert.Equal(title, updated.Title);
            Assert.Equal(startAt, updated.StartAt);
            Assert.Equal(endAt, updated.EndAt);
            Assert.Equal(description, updated.Description);
        }
        [Fact]
        public void UpdateEvent_WrongID_KeyNotFoundException()
        {
            var ex = Record.Exception(() => eventService.UpdateEventAsync(Guid.NewGuid(), "Event to update", DateTime.MinValue, DateTime.MaxValue).GetAwaiter().GetResult());

            Assert.NotNull(ex);
            Assert.IsType<KeyNotFoundException>(ex);
        }
        [Theory]
        [InlineData("Test event", "2026-04-02T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:15.444Z", "2026-04-01T11:24:14.444Z")]
        [InlineData("Test event", "2027-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z")]
        public void UpdateEvent_StartDateGreaterThenEndDate_ArgumentException(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var ev = eventService.AddEventAsync("Event to update", DateTime.MinValue, DateTime.MaxValue, 100).GetAwaiter().GetResult();

            var ex = Record.Exception(() => eventService.UpdateEventAsync(ev.Id,title, startAt, endAt, description).GetAwaiter().GetResult());

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);

        }
        [Theory]
        [InlineData("", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("12", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("TTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT",
    "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        public void UpdateEvent_InvalidTitle_ArgumentException(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var ev = eventService.AddEventAsync("Event to update", DateTime.MinValue, DateTime.MaxValue, 100).GetAwaiter().GetResult();

            var ex = Record.Exception(() => eventService.UpdateEventAsync(ev.Id, title, startAt, endAt, description).GetAwaiter().GetResult());

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);

        }

    }
}
