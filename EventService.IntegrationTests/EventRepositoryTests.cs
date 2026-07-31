using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Models;
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

        [Fact]
        public async Task Update_Works()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var ev = DomainEvent.Create(Guid.NewGuid(), "ToUpdate", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 5, 5);
            var added = await repo.AddAsync(ev);

            var newStart = added.StartAt.AddHours(1);
            var newEnd = added.EndAt.AddHours(1);
            added.UpdateEvent("Updated title", newStart, newEnd, "Updated description");

            await repo.UpdateAsync(added);

            var fetched = await repo.GetByIdAsync(added.Id);
            Assert.Equal("Updated title", fetched.Title);
            Assert.Equal("Updated description", fetched.Description);
            Assert.Equal(newStart, fetched.StartAt);
            Assert.Equal(newEnd, fetched.EndAt);
        }

        [Fact]
        public async Task Exists_Works()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var ev = DomainEvent.Create(Guid.NewGuid(), "ExistsEvent", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 5, 5);
            var added = await repo.AddAsync(ev);

            var exists = await repo.ExistsAsync(added.Id);
            Assert.True(exists);

            var notExists = await repo.ExistsAsync(Guid.NewGuid());
            Assert.False(notExists);
        }

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            var context = new AppDbContext(options);

            context.Database.Migrate();
            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            NpgsqlConnection.ClearAllPools();
            await using var ctx = CreateContext();

            try
            {
                ctx.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Bookings\" CASCADE;");
                ctx.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Events\" CASCADE;");
            }
            catch
            {
            }
        }

        private EventRepository CreateRepository(AppDbContext context) => new EventRepository(context);

        [Fact]
        public async Task AddAndGetById_Succeeds()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var ev = DomainEvent.Create(Guid.NewGuid(), "Testing event", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, 10);
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

            for (int i = 0; i < 25; i++)
            {
                var title = i % 2 == 0 ? $"EvenEvent-{i}" : $"OddEvent-{i}";
                var start = DateTime.UtcNow.Date.AddDays(i);
                var end = start.AddHours(1);
                await repo.AddAsync(DomainEvent.Create(Guid.NewGuid(), title, start, end, 10, 10));
            }

            var result = await repo.GetAllAsync(new EventsFilters(null, null, null), new EventManagerService.Domain.ValueObjects.Paginations(10, 1));
            Assert.Equal(25, result.Total);
            Assert.Equal(10, result.Items.Count);

            var result2 = await repo.GetAllAsync(new EventsFilters("Odd", null, null), new EventManagerService.Domain.ValueObjects.Paginations(100, 1));
            Assert.Equal(12, result2.Total);
            Assert.All(result2.Items, i => Assert.Contains("Odd", i.Title));

            var from = DateTime.UtcNow.Date.AddDays(5);
            var to = DateTime.UtcNow.Date.AddDays(15);
            var result3 = await repo.GetAllAsync(new EventsFilters(null, from, to), new EventManagerService.Domain.ValueObjects.Paginations(100, 1));
            Assert.True(result3.Total >= 1);
            Assert.All(result3.Items, i => Assert.True(i.StartAt >= from && i.EndAt <= to));

            var result4 = await repo.GetAllAsync(new EventsFilters(null, null, null), new EventManagerService.Domain.ValueObjects.Paginations(10, 3));
            Assert.Equal(25, result4.Total);
            Assert.Equal(5, result4.Items.Count);
        }

        [Fact]
        public async Task TryReserveAndReleaseSeats_Works()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var ev = DomainEvent.Create(Guid.NewGuid(), "SeatsEvent", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 5, 5);
            var added = await repo.AddAsync(ev);

            added.TryReserveSeats(3);
            var ok = await repo.TryReserveSeatsAsync(added);
            Assert.True(ok);


            var fetched = await repo.GetByIdAsync(added.Id);
            await Assert.ThrowsAsync<EventManagerService.Domain.Exceptions.NoAvailableSeatsException>(async () => await Task.Run(() => fetched.TryReserveSeats(3)));

            var fetchedForRelease = await repo.GetByIdAsync(added.Id);
            fetchedForRelease.ReleaseSeats(2);
            var rel = await repo.ReleaseSeatsAsync(fetchedForRelease);
            Assert.True(rel);


            var fetchedTooMuch = await repo.GetByIdAsync(added.Id);
            await Assert.ThrowsAsync<EventManagerService.Domain.Exceptions.GreaterThenValidationException>(async () => await Task.Run(() => fetchedTooMuch.ReleaseSeats(100)));
        }

        [Fact]
        public async Task Delete_Works()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var ev = DomainEvent.Create(Guid.NewGuid(), "ToDelete", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 5, 5);
            var added = await repo.AddAsync(ev);

            await repo.DeleteAsync(added.Id);

            await Assert.ThrowsAsync<EventManagerService.Shared.Exceptions.AppException>(async () => await repo.GetByIdAsync(added.Id));
        }
    }
}
