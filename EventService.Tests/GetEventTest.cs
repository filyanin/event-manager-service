using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EventService.Tests
{
    public  class GetEventTest
    {
        public IEventService eventService;
        public List<Event> eventList;

        public GetEventTest()
        {

            eventService = new EventManagerService.Domain.EventService();
            eventService.AddEvent("Test event", DateTime.MinValue, DateTime.MaxValue);


            //Получение приватного поля eventList для прямой проверки на наличие объекта
            Type type = typeof(EventManagerService.Domain.EventService);
            var field = type.GetField("events", BindingFlags.Instance | BindingFlags.NonPublic);
            eventList = (List<Event>)field?.GetValue(eventService);
        }
        [Fact]
        public void SuccessGetEventById_GetEventTest()
        {
            var ev = eventService.AddEvent("Event to update", DateTime.MinValue, DateTime.MaxValue);

            var anotherEvent = eventService.GetEventById(ev.Id);

            Assert.NotNull(anotherEvent);
            Assert.Equal(ev.Id, anotherEvent.Id);
        }
        [Fact]
        public void WrongId_GetEventTest()
        {
            var ex = Record.Exception(() => eventService.GetEventById(Guid.NewGuid()));

            Assert.NotNull(ex);
            Assert.IsType<KeyNotFoundException>(ex);
        }

    }
}
