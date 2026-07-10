using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;

namespace EventService.Tests
{
    public class GetAllEventTest
    {
        public IEventService eventService;
        public AppDbContext _context;
        public List<string> titles;
        public GetAllEventTest()
        {
            var options = new DbContextOptionsBuilder<EventManagerService.Infrastructure.DataAssets.AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new EventManagerService.Infrastructure.DataAssets.AppDbContext(options);
            // Используем репозиторий поверх InMemory DbContext и передаём его в сервис
            var eventRepo = new EventManagerService.Infrastructure.Repositories.EventRepository(_context);
            eventService = new EventManagerService.Domain.Services.EventService.EventService(eventRepo);
            eventService.AddEventAsync("Good Event To Test", DateTime.Parse("2026-04-01T11:24:14.444Z"), DateTime.Parse("2026-04-02T11:24:14.444Z"),1).GetAwaiter().GetResult();
            eventService.AddEventAsync("Bad Event To Test", DateTime.Parse("2026-04-02T11:24:14.444Z"), DateTime.Parse("2026-04-03T11:24:14.444Z"), 1).GetAwaiter().GetResult();
            eventService.AddEventAsync("Simple Event To Test", DateTime.Parse("2026-04-03T11:24:14.444Z"), DateTime.Parse("2026-04-04T11:24:14.444Z"), 1).GetAwaiter().GetResult();
            eventService.AddEventAsync("Gooooood Event To Test", DateTime.Parse("2026-04-04T11:24:14.444Z"), DateTime.Parse("2026-04-05T11:24:14.444Z"), 1).GetAwaiter().GetResult();
            eventService.AddEventAsync("Simple Event", DateTime.Parse("2026-04-05T11:24:14.444Z"), DateTime.Parse("2026-04-06T11:24:14.444Z"), 1).GetAwaiter().GetResult();

            titles = new List<string>();
            titles.Add("Good Event To Test");
            titles.Add("Bad Event To Test");
            titles.Add("Simple Event To Test");
            titles.Add("Gooooood Event To Test");
            titles.Add("Simple Event");
        }
        [Fact] 
        public async Task GetAllEvent_EmptyFilters_SuccessGetAllEvents()
        {
            var tuple = await eventService.GetAllEventAsync(new EventManagerService.Domain.Filters.EventsFilters(null, null, null), 1, 10);
            var total = await _context.Events.CountAsync();
            Assert.Equal(total, tuple.Total);
            Assert.All(tuple.Items, e => titles.Contains(e.Title));
        }
        [Fact]
        public async Task GetAllEvent_FilterByTitle_SuccessGetFilteredEvents()
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

            var tuple = await eventService.GetAllEventAsync(new EventManagerService.Domain.Filters.EventsFilters(searchSubstring, null, null), 1, 10);

            Assert.Equal(expectedResult.Count, tuple.Total);
            Assert.All(tuple.Items, e => expectedResult.Contains(e.Title));
            Assert.True(tuple.Items.All(e => !notExpectedResult.Contains(e.Title)));

        }

        [Fact]
        public async Task GetAllEvent_FilterByStartDate_SuccessGetFilteredEvents()
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

            var tuple = await eventService.GetAllEventAsync(new EventManagerService.Domain.Filters.EventsFilters(null, searchDate, null), 1, 10);

            Assert.Equal(expectedResult.Count, tuple.Total);
            Assert.All(tuple.Items, e => expectedResult.Contains(e.Title));
            Assert.True(tuple.Items.All(e => !notExpectedResult.Contains(e.Title)));

        }

        [Fact]
        public async Task GetAllEvent_FilterByEndDate_SuccessGetFilteredEvents()
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

            var tuple = await eventService.GetAllEventAsync(new EventManagerService.Domain.Filters.EventsFilters(null, null, searchDate), 1, 10);

            Assert.Equal(expectedResult.Count, tuple.Total);
            Assert.All(tuple.Items, e => expectedResult.Contains(e.Title));
            Assert.True(tuple.Items.All(e => !notExpectedResult.Contains(e.Title)));

        }

        [Fact]
        public async Task GetAllEvent_FilterByStartAndEndDate_SuccessGetFilteredEvents()
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

            var tuple = await eventService.GetAllEventAsync(new EventManagerService.Domain.Filters.EventsFilters(null, startDate, endDate), 1, 10);

            Assert.Equal(expectedResult.Count, tuple.Total);
            Assert.All(tuple.Items, e => expectedResult.Contains(e.Title));
            Assert.True(tuple.Items.All(e => !notExpectedResult.Contains(e.Title)));
        }
        [Fact]
        public async Task GetAllEvent_FilterByTitleAndStartAndEndDate_SuccessGetFilteredEvents()
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

            var tuple = await eventService.GetAllEventAsync(new EventManagerService.Domain.Filters.EventsFilters(title, startDate, endDate), 1, 10);

            Assert.Equal(expectedResult.Count, tuple.Total);
            Assert.All(tuple.Items, e => expectedResult.Contains(e.Title));
            Assert.True(tuple.Items.All(e => !notExpectedResult.Contains(e.Title)));
        }

        [Theory]
        [InlineData(100,10,1, 10)]
        [InlineData(8, 10, 1, 8)]
        public async Task GetAllEvent_PaginationData_SuccessGetFilteredEvents(int elementCounts, int pageSize, int pageNumber, int expectedPageSize)
        {
            var options = new DbContextOptionsBuilder<EventManagerService.Infrastructure.DataAssets.AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new EventManagerService.Infrastructure.DataAssets.AppDbContext(options);
            var eventRepo = new EventManagerService.Infrastructure.Repositories.EventRepository(context);
            var service = new EventManagerService.Domain.Services.EventService.EventService(eventRepo);
            for (int i = 0; i < elementCounts; i++) 
            {
                service.AddEventAsync("TestEvent", DateTime.MinValue, DateTime.MaxValue, 1).GetAwaiter().GetResult();
            }

            var tuple = await service.GetAllEventAsync(new EventManagerService.Domain.Filters.EventsFilters(null, null, null), pageNumber, pageSize);

            Assert.Equal(elementCounts, tuple.Total);
            Assert.Equal(expectedPageSize, tuple.Items.Count);
        }


    }
}
