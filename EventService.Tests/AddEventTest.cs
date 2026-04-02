using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Models;
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
            eventService = new EventManagerService.Domain.EventService();

            //Получение приватного поля eventList для прямой проверки на наличие объекта
            Type type = typeof(EventManagerService.Domain.EventService);
            var field = type.GetField("events", BindingFlags.Instance | BindingFlags.NonPublic);
            eventsList = (List<Event>)field?.GetValue(eventService);
        }

        [Theory]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z", "Test description")]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-01T11:24:15.444Z", "Test description")]
        [InlineData("Test event", "2025-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z", "Test description")]
        public void AddEvent_CorrectInputData_SuccessCreate(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            
            var ev = eventService.AddEvent(title, startAt, endAt, description);

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
        public void AddEvent_WrongTitleString_ArgumentException(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var ex = Record.Exception(() => eventService.AddEvent(title, startAt, endAt, description));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
        }


        [Theory]
        [InlineData("Test event", "2026-04-02T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z")]
        [InlineData("Test event", "2026-04-01T11:24:15.444Z", "2026-04-01T11:24:14.444Z")]
        [InlineData("Test event", "2027-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z")]
        public void AddEvent_StartDateGreaterThenEndDate_ArgumentException(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var ex = Record.Exception(() => eventService.AddEvent(title, startAt, endAt, description));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
        }
    }
}
