using EventManagerService.Domain;
using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Models;
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
            //Создание EventManager с непустым списком
            eventService = new EventManagerService.Domain.EventService();
            eventService.AddEvent(Event.Create("Test event", DateTime.MinValue, DateTime.MaxValue));


            //Получение приватного поля eventList для прямой проверки на наличие объекта
            Type type = typeof(EventManagerService.Domain.EventService);
            var field = type.GetField("events", BindingFlags.Instance | BindingFlags.NonPublic);
            eventList = (List<Event>)field?.GetValue(eventService);
        }

        [Fact]
        public void SuccessDeleteEventById_DeleteEventTest()
        {

            var ev = Event.Create("Test event", DateTime.MinValue, DateTime.MaxValue);
            eventService.AddEvent(ev);

            eventService.DeleteEvent(ev.Id);


            Assert.DoesNotContain<Event>(ev, eventList);

        }

        public void WrongId_DeleteEventTest()
        {

        }

    }
}
