using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Models.DomainBooking;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Npgsql;
using Xunit;
using EventManagerService.Infrastructure.Repositories;

namespace EventService.IntegrationTests
{
    public class BookingRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
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

        [Fact]
        public async Task CreateAndGetByStateAndConfirmReject_Works()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var eventRepo = new EventManagerService.Infrastructure.Repositories.EventRepository(context);
            var bookingRepo = new EventManagerService.Infrastructure.Repositories.BookingRepository(context);

            var ev = EventManagerService.Domain.Models.DomainEvent.DomainEvent.Create("BookingEvent", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 5);
            var added = await eventRepo.AddAsync(ev);

            var booking = await bookingRepo.CreateAsync(added.Id);
            Assert.Equal(BookingStatus.Pending, booking.Status);

            var pending = await bookingRepo.GetByStateAsync(BookingStatus.Pending);
            Assert.Contains(pending, b => b.Id == booking.Id);

            await bookingRepo.ConfirmAsync(booking.Id);
            var byId = await bookingRepo.GetByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Confirmed, byId.Status);

            // create another
            var booking2 = await bookingRepo.CreateAsync(added.Id);
            await bookingRepo.RejectAsync(booking2.Id);
            var byId2 = await bookingRepo.GetByIdAsync(booking2.Id);
            Assert.Equal(BookingStatus.Rejected, byId2.Status);
        }

        [Fact]
        public async Task GetPendingIds_Works()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var eventRepo = new EventManagerService.Infrastructure.Repositories.EventRepository(context);
            var bookingRepo = new EventManagerService.Infrastructure.Repositories.BookingRepository(context);

            var ev = EventManagerService.Domain.Models.DomainEvent.DomainEvent.Create("BookingEvent2", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 5);
            var added = await eventRepo.AddAsync(ev);

            var booking = await bookingRepo.CreateAsync(added.Id);
            var pendingIds = await bookingRepo.GetPendingIdsAsync(default);
            Assert.Contains(booking.Id, pendingIds);
        }
    }
}