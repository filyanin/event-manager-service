using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Models.Event;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EventService.Tests
{
    public  class DeleteEventTest
    {

        public IEventService eventService;
        public List<Event> eventList;

        public DeleteEventTest() 
        {

            eventService = new EventManagerService.Domain.Services.EventService.EventService();
            eventService.AddEvent("Test event", DateTime.MinValue, DateTime.MaxValue);


            //Получение приватного поля eventList для прямой проверки на наличие объекта
            Type type = typeof(EventManagerService.Domain.Services.EventService.EventService);
            var field = type.GetField("events", BindingFlags.Instance | BindingFlags.NonPublic);
            eventList = (List<Event>)field?.GetValue(eventService);
        }

        [Fact]
        public void DeleteEvent_CorrectId_SuccessDelete()
        {

            var ev = eventService.AddEvent("Test event", DateTime.MinValue, DateTime.MaxValue);

            eventService.DeleteEvent(ev.Id);


            Assert.DoesNotContain<Event>(ev, eventList);

        }
        [Fact]
        public void DeleteEvent_WrongId_KeyNotFoundException()
        {
            var ex = Record.Exception(() => eventService.DeleteEvent(Guid.NewGuid()));

            Assert.NotNull(ex);
            Assert.IsType<KeyNotFoundException>(ex);
        }

    }
}
