using EventManagerService.Domain.Enum;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Npgsql;
using Xunit;
using EventManagerService.Domain.Models;

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

        [Fact]
        public async Task CreateAndGetByStateAndConfirmReject_Works()
        {
            await ResetDatabaseAsync();
            using var context = CreateContext();
            var eventRepo = new EventManagerService.Infrastructure.Repositories.EventRepository(context);
            var bookingRepo = new EventManagerService.Infrastructure.Repositories.BookingRepository(context);
            var userRepo = new EventManagerService.Infrastructure.Repositories.UserRepository(context);

            var user = await userRepo.CreateAsync("testuser", "11111");

            var ev = DomainEvent.Create(Guid.NewGuid(), "BookingEvent", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 5, 5);
            var added = await eventRepo.AddAsync(ev);

            var bookingToCreate = new DomainBooking(added.Id, user.Id);
            var booking = await bookingRepo.CreateAsync(bookingToCreate);
            Assert.Equal(BookingStatus.Pending, booking.Status);

            var pending = await bookingRepo.GetByStateAsync(BookingStatus.Pending);
            Assert.Contains(pending, b => b.Id == booking.Id);

            booking.SetBookingConfirmed(DateTime.UtcNow.AddMinutes(1));
            await bookingRepo.ChangeBookingStateAsync(booking);
            var byId = await bookingRepo.GetByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Confirmed, byId.Status);

            
            var booking2ToCreate = new DomainBooking(added.Id, user.Id);
            var booking2 = await bookingRepo.CreateAsync(booking2ToCreate);
            booking2.SetBookingRejected(DateTime.UtcNow.AddMinutes(2));
            await bookingRepo.ChangeBookingStateAsync(booking2);
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
            var userRepo = new EventManagerService.Infrastructure.Repositories.UserRepository(context);

            var user = await userRepo.CreateAsync("testuser", "11111");

            var ev = DomainEvent.Create(Guid.NewGuid(), "BookingEvent2", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 5, 5);
            var added = await eventRepo.AddAsync(ev);

            var bookingToCreate = new DomainBooking(added.Id, user.Id);
            var booking = await bookingRepo.CreateAsync(bookingToCreate);
            var pendingIds = await bookingRepo.GetPendingIdsAsync(default);
            Assert.Contains(booking.Id, pendingIds);
        }
    }
}
