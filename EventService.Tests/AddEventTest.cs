using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Models.Event;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Xunit;


namespace EventService.Tests
{
    public class AddEventTest
    {
        public IEventService eventService;
        public List<Event> eventsList;  
        public AddEventTest()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new AppDbContext(options);
            eventService = new EventManagerService.Domain.Services.EventService.EventService(context);

            //Получение приватного поля eventList для прямой проверки на наличие объекта
            Type type = typeof(EventManagerService.Domain.Services.EventService.EventService);
            var field = type.GetField("events", BindingFlags.Instance | BindingFlags.NonPublic);
            eventsList = (List<Event>)field?.GetValue(eventService);
        }

        [Theory]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z", "Test description")]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-01T11:24:15.444Z", "Test description")]
        [InlineData("Test event", "2025-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z", "Test description")]
        public async Task AddEvent_CorrectInputData_SuccessCreate(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var ev = await eventService.AddEventAsync(title, startAt, endAt, 100, description);

            Assert.NotNull(ev);
            Assert.Equal(title, ev.Title);
            Assert.Equal(startAt, ev.StartAt);
            Assert.Equal(endAt, ev.EndAt);
            Assert.Equal(description, ev.Description);
            Assert.Contains(ev,eventsList);
        }
        
        
        [Theory]
        [InlineData("", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("12", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("TTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT",
            "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        public async Task AddEvent_WrongTitleString_ArgumentException(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => eventService.AddEventAsync(title, startAt, endAt, 100, description));
        }


        [Theory]
        [InlineData("Test event", "2026-04-02T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:15.444Z", "2026-04-01T11:24:14.444Z")]
        [InlineData("Test event", "2027-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z")]
        public async Task AddEvent_StartDateGreaterThenEndDate_ArgumentException(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => eventService.AddEventAsync(title, startAt, endAt, 100, description));
        }
    }
}
