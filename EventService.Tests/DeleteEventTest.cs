using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Models.Event;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
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
            var options = new DbContextOptionsBuilder<EventManagerService.Infrastructure.DataAssets.AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new EventManagerService.Infrastructure.DataAssets.AppDbContext(options);
            eventService = new EventManagerService.Domain.Services.EventService.EventService(context);
            eventService.AddEventAsync("Test event", DateTime.MinValue, DateTime.MaxValue, 100).GetAwaiter().GetResult();


            //Получение приватного поля eventList для прямой проверки на наличие объекта
            Type type = typeof(EventManagerService.Domain.Services.EventService.EventService);
            var field = type.GetField("events", BindingFlags.Instance | BindingFlags.NonPublic);
            eventList = (List<Event>)field?.GetValue(eventService);
        }

        [Fact]
        public async Task DeleteEvent_CorrectId_SuccessDelete()
        {

            var ev = await eventService.AddEventAsync("Test event", DateTime.MinValue, DateTime.MaxValue, 100);

            await eventService.DeleteEventAsync(ev.Id);


            Assert.DoesNotContain<Event>(ev, eventList);

        }
        [Fact]
        public async Task DeleteEvent_WrongId_KeyNotFoundException()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => eventService.DeleteEventAsync(Guid.NewGuid()));
        }

    }
}
