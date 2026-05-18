using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Models.Event;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EventService.Tests
{
    public  class GetEventByIdTest
    {
        public IEventService eventService;
        public List<Event> eventList;

        public GetEventByIdTest()
        {

            eventService = new EventManagerService.Domain.Services.EventService.EventService();
            eventService.AddEvent("Test event", DateTime.MinValue, DateTime.MaxValue);


            //Получение приватного поля eventList для прямой проверки на наличие объекта
            Type type = typeof(EventManagerService.Domain.Services.EventService.EventService);
            var field = type.GetField("events", BindingFlags.Instance | BindingFlags.NonPublic);
            eventList = (List<Event>)field?.GetValue(eventService);
        }
        [Fact]
        public void GetEventById_CorrectId_SuccessGetEvent()
        {
            var ev = eventService.AddEvent("Event to update", DateTime.MinValue, DateTime.MaxValue);

            var anotherEvent = eventService.GetEventById(ev.Id);

            Assert.NotNull(anotherEvent);
            Assert.Equal(ev.Id, anotherEvent.Id);
        }
        [Fact]
        public void GetEventById_WrongId_KeyNotFoundException()
        {
            var ex = Record.Exception(() => eventService.GetEventById(Guid.NewGuid()));

            Assert.NotNull(ex);
            Assert.IsType<KeyNotFoundException>(ex);
        }

    }
}
