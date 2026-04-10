using EventManagerService.Domain;
using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EventService.Tests
{
    public class GetAllEventTest
    {
        public IEventService eventService;
        public List<Event> eventList;
        public List<string> titles;
        public GetAllEventTest()
        {

            eventService = new EventManagerService.Domain.EventService();
            eventService.AddEvent("Good Event To Test", DateTime.Parse("2026-04-01T11:24:14.444Z"), DateTime.Parse("2026-04-02T11:24:14.444Z"));
            eventService.AddEvent("Bad Event To Test", DateTime.Parse("2026-04-02T11:24:14.444Z"), DateTime.Parse("2026-04-03T11:24:14.444Z"));
            eventService.AddEvent("Simple Event To Test", DateTime.Parse("2026-04-03T11:24:14.444Z"), DateTime.Parse("2026-04-04T11:24:14.444Z"));
            eventService.AddEvent("Gooooood Event To Test", DateTime.Parse("2026-04-04T11:24:14.444Z"), DateTime.Parse("2026-04-05T11:24:14.444Z"));
            eventService.AddEvent("Simple Event", DateTime.Parse("2026-04-05T11:24:14.444Z"), DateTime.Parse("2026-04-06T11:24:14.444Z"));

            titles = new List<string>();
            titles.Add("Good Event To Test");
            titles.Add("Bad Event To Test");
            titles.Add("Simple Event To Test");
            titles.Add("Gooooood Event To Test");
            titles.Add("Simple Event");

            Type type = typeof(EventManagerService.Domain.EventService);
            var field = type.GetField("events", BindingFlags.Instance | BindingFlags.NonPublic);
            eventList = (List<Event>)field?.GetValue(eventService);
        }
        [Fact] 
        public void GetAllEvent_EmptyFilters_SuccessGetAllEvents()
        {
            int totalCount;
            var result = eventService.GetAllEvent(out totalCount,
                new EventManagerService.Domain.Filters.EventsFilters(null, null, null), 1, 10);

            Assert.Equal(eventList.Count, totalCount);
            Assert.All(result, e => titles.Contains(e.Title));
        }
        [Fact]
        public void GetAllEvent_FilterByTitle_SuccessGetFilteredEvents()
        {
            var searchSubstring = "Goo";
            List<string> expectedResult = new List<string>
            {
                "Good Event To Test",
                "Gooooood Event To Test"
            };
            List<string> notExpectedResult = new List<string>
            {
                "Bad Event To Test",
                "Simple Event To Test",
                "Simple Event"
            };

            int totalCount;
            var result = eventService.GetAllEvent(out totalCount,
                new EventManagerService.Domain.Filters.EventsFilters(searchSubstring, null, null), 1, 10);

            Assert.Equal(expectedResult.Count, totalCount);
            Assert.All(result, e => expectedResult.Contains(e.Title));
            Assert.True(result.All(e => !notExpectedResult.Contains(e.Title)));

        }

        [Fact]
        public void GetAllEvent_FilterByStartDate_SuccessGetFilteredEvents()
        {
            var searchDate = DateTime.Parse("2026-04-03T11:24:14.444Z");

            List<string> expectedResult = new List<string>
            {
                "Simple Event To Test",
                "Gooooood Event To Test",
                "Simple Event"


            };
            List<string> notExpectedResult = new List<string>
            {

                "Good Event To Test",
                "Bad Event To Test"

            };

            int totalCount;
            var result = eventService.GetAllEvent(out totalCount,
                new EventManagerService.Domain.Filters.EventsFilters(null, searchDate, null), 1, 10);

            Assert.Equal(expectedResult.Count, totalCount);
            Assert.All(result, e => expectedResult.Contains(e.Title));
            Assert.True(result.All(e => !notExpectedResult.Contains(e.Title)));

        }

        [Fact]
        public void GetAllEvent_FilterByEndDate_SuccessGetFilteredEvents()
        {
            var searchDate = DateTime.Parse("2026-04-03T11:24:14.444Z");

            List<string> expectedResult = new List<string>
            {
                "Good Event To Test",
                "Bad Event To Test"


            };
            List<string> notExpectedResult = new List<string>
            {
                "Simple Event To Test",
                "Gooooood Event To Test",
                "Simple Event"

            };

            int totalCount;
            var result = eventService.GetAllEvent(out totalCount,
                new EventManagerService.Domain.Filters.EventsFilters(null, null, searchDate), 1, 10);

            Assert.Equal(expectedResult.Count, totalCount);
            Assert.All(result, e => expectedResult.Contains(e.Title));
            Assert.True(result.All(e => !notExpectedResult.Contains(e.Title)));

        }

        [Fact]
        public void GetAllEvent_FilterByStartAndEndDate_SuccessGetFilteredEvents()
        {
            var startDate = DateTime.Parse("2026-04-02T11:24:15.444Z");
            var endDate = DateTime.Parse("2026-04-05T11:24:13.444Z");

            List<string> expectedResult = new List<string>
            {
                "Simple Event To Test"
                


            };
            List<string> notExpectedResult = new List<string>
            {
                "Good Event To Test",
                "Bad Event To Test",
                "Gooooood Event To Test",
                "Simple Event"

            };

            int totalCount;
            var result = eventService.GetAllEvent(out totalCount,
                new EventManagerService.Domain.Filters.EventsFilters(null, startDate, endDate), 1, 10);

            Assert.Equal(expectedResult.Count, totalCount);
            Assert.All(result, e => expectedResult.Contains(e.Title));
            Assert.True(result.All(e => !notExpectedResult.Contains(e.Title)));
        }
        [Fact]
        public void GetAllEvent_FilterByTitleAndStartAndEndDate_SuccessGetFilteredEvents()
        {
            var startDate = DateTime.Parse("2026-04-02T11:24:14.444Z");
            var endDate = DateTime.Parse("2026-04-06T11:24:14.444Z");
            var title = "Goo";

            List<string> expectedResult = new List<string>
            {
                "Gooooood Event To Test"
                
            };
            List<string> notExpectedResult = new List<string>
            {
                "Good Event To Test",
                "Bad Event To Test",
                "Simple Event To Test",
                "Simple Event"

            };

            int totalCount;
            var result = eventService.GetAllEvent(out totalCount,
                new EventManagerService.Domain.Filters.EventsFilters(title, startDate, endDate), 1, 10);

            Assert.Equal(expectedResult.Count, totalCount);
            Assert.All(result, e => expectedResult.Contains(e.Title));
            Assert.True(result.All(e => !notExpectedResult.Contains(e.Title)));
        }

        [Theory]
        [InlineData(100,10,1, 10)]
        [InlineData(8, 10, 1, 8)]
        public void GetAllEvent_PaginationData_SuccessGetFilteredEvents(int elementCounts, int pageSize, int pageNumber, int expectedPageSize)
        {
            var service = new EventManagerService.Domain.EventService();
            for (int i = 0; i < elementCounts; i++) 
            {
                service.AddEvent("TestEvent", DateTime.MinValue, DateTime.MaxValue);
            }
            
            
            int totalCount;
            var result = service.GetAllEvent(out totalCount, new EventManagerService.Domain.Filters.EventsFilters(null, null, null), pageNumber, pageSize);



            Assert.Equal(elementCounts, totalCount);
            Assert.Equal(expectedPageSize, result.Count);
        }


    }
}
