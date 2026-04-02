using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static System.Net.WebRequestMethods;

namespace EventService.Tests
{
    public  class UpdateEventTest
    {
        public IEventService eventService;
        public List<Event> eventList;

        public UpdateEventTest()
        {

            eventService = new EventManagerService.Domain.EventService();
            eventService.AddEvent("Test event", DateTime.MinValue, DateTime.MaxValue);


            //Получение приватного поля eventList для прямой проверки на наличие объекта
            Type type = typeof(EventManagerService.Domain.EventService);
            var field = type.GetField("events", BindingFlags.Instance | BindingFlags.NonPublic);
            eventList = (List<Event>)field?.GetValue(eventService);
        }

        [Theory]
        [InlineData("New Good event", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z")]
        [InlineData("New Good event", "2026-04-01T11:24:14.444Z", "2026-04-02T11:24:14.444Z", "Test description")]
        [InlineData("New Good event", "2026-04-01T11:24:14.444Z", "2026-04-01T11:24:15.444Z", "Test description")]
        [InlineData("New Good event", "2025-04-01T11:24:14.444Z", "2026-04-01T11:24:14.444Z", "Test description")]
        public void UpdateEvent_CorrectInputData_SuccessUpdateEvent(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            var ev = eventService.AddEvent("Event to update",DateTime.MinValue, DateTime.MaxValue);

            eventService.UpdateEvent(ev.Id, title, startAt, endAt, description);

            Assert.Equal(title, ev.Title);
            Assert.Equal(startAt, ev.StartAt);
            Assert.Equal(endAt, ev.EndAt);
            Assert.Equal(description, ev.Description);
        }
        [Fact]
        public void UpdateEvent_WrongID_KeyNotFoundException()
        {
            var ex = Record.Exception(() => eventService.UpdateEvent(Guid.NewGuid(), "Event to update", DateTime.MinValue, DateTime.MaxValue));

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
            var ev = eventService.AddEvent("Event to update", DateTime.MinValue, DateTime.MaxValue);

            var ex = Record.Exception(() => eventService.UpdateEvent(ev.Id,title, startAt, endAt, description));

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
            var ev = eventService.AddEvent("Event to update", DateTime.MinValue, DateTime.MaxValue);

            var ex = Record.Exception(() => eventService.UpdateEvent(ev.Id, title, startAt, endAt, description));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);

        }

    }
}
