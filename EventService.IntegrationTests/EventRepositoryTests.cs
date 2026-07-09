using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models.DomainEvent;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using EventManagerService.Infrastructure.Repositories;
using Npgsql;
using Xunit;

namespace EventService.IntegrationTests
{
    public class EventRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithCleanUp(true)
            .Build();

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            var context = new AppDbContext(options);
            // ensure database exists
            context.Database.EnsureCreated();
            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            // clear pooled connections so database can be dropped
            NpgsqlConnection.ClearAllPools();
            await using var ctx = CreateContext();
            ctx.Database.EnsureCreated();
            try
            {
                ctx.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Bookings\" CASCADE;");
                ctx.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Events\" CASCADE;");
            }
            catch
            {
                // ignore if tables don't exist yet
            }
        }

        private EventRepository CreateRepository(AppDbContext context) => new EventRepository(context);

        [Fact]
        public async Task AddAndGetById_Succeeds()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var ev = DomainEvent.Create("Testing event", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10);
            var added = await repo.AddAsync(ev);

            var fetched = await repo.GetByIdAsync(added.Id);

            Assert.Equal(added.Id, fetched.Id);
            Assert.Equal("Testing event", fetched.Title);
        }

        [Fact]
        public async Task GetAll_FiltersAndPagination_Works()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var repo = CreateRepository(context);

            // seed 25 events with varying titles and dates
            for (int i = 0; i < 25; i++)
            {
                // ensure title meets domain minimum length (>=6)
                var title = i % 2 == 0 ? $"EvenEvent-{i}" : $"OddEvent-{i}";
                var start = DateTime.UtcNow.Date.AddDays(i);
                var end = start.AddHours(1);
                await repo.AddAsync(DomainEvent.Create(title, start, end, 10));
            }

            // no filters, page 1 size 10
            var result = await repo.GetAllAsync(new EventsFilters(null, null, null), 1, 10);
            Assert.Equal(25, result.Total);
            Assert.Equal(10, result.Items.Count);

            // filter by title substring
            var result2 = await repo.GetAllAsync(new EventsFilters("Odd", null, null), 1, 100);
            Assert.Equal(12, result2.Total);
            Assert.All(result2.Items, i => Assert.Contains("Odd", i.Title));

            // filter by date range
            var from = DateTime.UtcNow.Date.AddDays(5);
            var to = DateTime.UtcNow.Date.AddDays(15);
            var result3 = await repo.GetAllAsync(new EventsFilters(null, from, to), 1, 100);
            Assert.True(result3.Total >= 1);
            Assert.All(result3.Items, i => Assert.True(i.StartAt >= from && i.EndAt <= to));

            // pagination boundary
            var result4 = await repo.GetAllAsync(new EventsFilters(null, null, null), 3, 10);
            Assert.Equal(25, result4.Total);
            Assert.Equal(5, result4.Items.Count);
        }

        [Fact]
        public async Task TryReserveAndReleaseSeats_Works()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var ev = DomainEvent.Create("SeatsEvent", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 5);
            var added = await repo.AddAsync(ev);

            var ok = await repo.TryReserveSeatsAsync(added.Id, 3);
            Assert.True(ok);

            var ok2 = await repo.TryReserveSeatsAsync(added.Id, 3);
            Assert.False(ok2);

            var rel = await repo.ReleaseSeatsAsync(added.Id, 2);
            Assert.True(rel);

            var rel2 = await repo.ReleaseSeatsAsync(added.Id, 10);
            Assert.False(rel2);
        }

        [Fact]
        public async Task Delete_Works()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var ev = DomainEvent.Create("ToDelete", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 5);
            var added = await repo.AddAsync(ev);

            await repo.DeleteAsync(added.Id);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await repo.GetByIdAsync(added.Id));
        }
    }
}
