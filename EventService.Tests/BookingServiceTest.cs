using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Exceptions;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Models.DomainBooking;
using EventManagerService.Domain.Models.DomainEvent;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EventManagerService.Infrastructure.Interfaces.Repositories;
using EventManagerService.Domain.Interfaces;
using EventManagerService.Domain.Models;
using EventManagerService.Application.Interfaces;
using EventManagerService.Application.Services;

namespace EventService.Tests
{
    public class BookingServiceTest
    {
        private readonly AppDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public BookingServiceTest()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));

            // регистрируем репозитории и сервисы домена как в продакшн
            services.AddScoped<IEventRepository, EventManagerService.Infrastructure.Repositories.EventRepository>();
            services.AddScoped<IBookingRepository, EventManagerService.Infrastructure.Repositories.BookingRepository>();
            services.AddScoped<IEventService, EventManagerService.Application.Services.EventService>();
            services.AddScoped<IBookingService, BookingService>();

            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<AppDbContext>();
            _eventService = _serviceProvider.GetRequiredService<IEventService>();
            _bookingService = _serviceProvider.GetRequiredService<IBookingService>();
        }

        private EventManagerService.Infrastructure.DataAssets.Models.Event CreateTestEvent(Guid eventId, int totalSeats = 100)
        {
            var domainEvent = DomainEvent.Create(eventId, "Test Event", DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2), totalSeats, totalSeats);
            var model = domainEvent.ConvertTo();
            _context.Events.Add(model);
            _context.SaveChanges();

            return model;
        }

        private void ReleaseSeatsInDb(Guid eventId, int count = 1)
        {
            var model = _context.Events.First(e => e.Id == eventId);
            model.AvailableSeats = Math.Min(model.TotalSeats, model.AvailableSeats + count);
            _context.Events.Update(model);
            _context.SaveChanges();
        }

        // Вспомогательный метод для разрешения сервисов в отдельном scope для параллельных тестов
        private TService ResolveScoped<TService>() where TService : notnull
        {
            using var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<TService>();
        }

        #region Basic Booking Tests

        [Fact]
        public async Task CreateBooking_EventExists_CreatesBooking()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(evId);

            Assert.NotNull(booking);
            Assert.Equal(evId, booking.EventId);
            Assert.Equal(BookingStatus.Pending, booking.Status);

            var fetched = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(booking.Id, fetched.Id);
        }

        [Fact]
        public async Task CreateBooking_EventNotExists_ThrowsKeyNotFoundException()
        {
            var evId = Guid.NewGuid();
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(evId));
        }

        [Fact]
        public async Task ConfirmBooking_ExistingBooking_ChangesStatusToConfirmed()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId);

            await _bookingService.ConfirmBookingAsync(booking.Id);

            var fetched = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Confirmed, fetched.Status);
            Assert.NotNull(fetched.ProcessedAt);
        }

        [Fact]
        public async Task RejectBooking_ExistingBooking_ChangesStatusToRejected()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId);

            await _bookingService.RejectBookingAsync(booking.Id);

            var fetched = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Rejected, fetched.Status);
            Assert.NotNull(fetched.ProcessedAt);
        }

        [Fact]
        public async Task ConfirmBooking_NonExistingBooking_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.ConfirmBookingAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateMultipleBookings_SameEvent_UniqueIds()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var b1 = await _bookingService.CreateBookingAsync(evId);
            var b2 = await _bookingService.CreateBookingAsync(evId);

            Assert.NotEqual(b1.Id, b2.Id);
            Assert.Equal(evId, b1.EventId);
            Assert.Equal(evId, b2.EventId);
        }

        [Fact]
        public async Task CreateBooking_EventDeletedBetweenCalls_SecondCreateThrows()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var first = await _bookingService.CreateBookingAsync(evId);
            Assert.NotNull(first);

            // Симулируем удаление события между вызовами
            var model = _context.Events.First(e => e.Id == evId);
            _context.Events.Remove(model);
            _context.SaveChanges();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(evId));
        }

        [Fact]
        public async Task GetBookingById_NonExistingId_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.GetBookingByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetBookingByState_ReturnsOnlyRequestedState()
        {
            var ev1 = Guid.NewGuid();
            var ev2 = Guid.NewGuid();
            CreateTestEvent(ev1);
            CreateTestEvent(ev2);

            var b1 = await _bookingService.CreateBookingAsync(ev1);
            var b2 = await _bookingService.CreateBookingAsync(ev2);

            await _bookingService.ConfirmBookingAsync(b1.Id);
            await _bookingService.RejectBookingAsync(b2.Id);

            var confirmed = await _bookingService.GetBookingByStateAsync(BookingStatus.Confirmed);
            Assert.Contains(confirmed, x => x.Id == b1.Id);
            Assert.DoesNotContain(confirmed, x => x.Id == b2.Id);

            var rejected = await _bookingService.GetBookingByStateAsync(BookingStatus.Rejected);
            Assert.Contains(rejected, x => x.Id == b2.Id);
            Assert.DoesNotContain(rejected, x => x.Id == b1.Id);
        }

        #endregion

        #region Seat Management Tests

        [Fact]
        public async Task CreateBooking_DecreasesAvailableSeats()
        {
            var evId = Guid.NewGuid();
            var @event = CreateTestEvent(evId, 100);
            int initialSeats = @event.AvailableSeats;

            var booking = await _bookingService.CreateBookingAsync(evId);

            Assert.Equal(initialSeats - 1, @event.AvailableSeats);
            Assert.NotNull(booking);
        }

        [Fact]
        public async Task CreateMultipleBookings_UpToLimit_AllSuccessful()
        {
            var evId = Guid.NewGuid();
            int totalSeats = 5;
            var @event = CreateTestEvent(evId, totalSeats);

            var bookings = new List<DomainBooking>();
            for (int i = 0; i < totalSeats; i++)
            {
                var booking = await _bookingService.CreateBookingAsync(evId);
                bookings.Add(booking);
                Assert.Equal(totalSeats - (i + 1), @event.AvailableSeats);
            }

            Assert.Equal(totalSeats, bookings.Count);
            Assert.Equal(0, @event.AvailableSeats);

            // Verify all have unique IDs
            var uniqueIds = new HashSet<Guid>(bookings.ConvertAll(b => b.Id));
            Assert.Equal(bookings.Count, uniqueIds.Count);
        }

        [Fact]
        public async Task CreateBooking_ExhaustedSeats_ThrowsNoAvailableSeatsException()
        {
            var evId = Guid.NewGuid();
            var @event = CreateTestEvent(evId, 1);

            // Create first booking - succeeds
            var booking1 = await _bookingService.CreateBookingAsync(evId);
            Assert.NotNull(booking1);
            Assert.Equal(0, @event.AvailableSeats);

            // Try to create second booking - should fail
            var exception = await Assert.ThrowsAsync<NoAvailableSeatsException>(
                () => _bookingService.CreateBookingAsync(evId));
            Assert.Equal("No available seats for this event", exception.Message);
        }

        [Fact]
        public async Task ReleaseSeats_RestoresAvailability()
        {
            var evId = Guid.NewGuid();
            int totalSeats = 10;
            var @event = CreateTestEvent(evId, totalSeats);

            // Резервируем 3 места
            await _bookingService.CreateBookingAsync(evId);
            await _bookingService.CreateBookingAsync(evId);
            await _bookingService.CreateBookingAsync(evId);
            Assert.Equal(totalSeats - 3, @event.AvailableSeats);

            // Освобождаем 2 места через вспомогательную функцию работы с БД
            ReleaseSeatsInDb(evId, 2);
            var modelAfterRelease = _context.Events.First(e => e.Id == evId);
            Assert.Equal(totalSeats - 1, modelAfterRelease.AvailableSeats);
        }

        [Fact]
        public async Task AfterRejectAndRelease_CanCreateNewBooking()
        {
            var evId = Guid.NewGuid();
            int totalSeats = 2;
            var @event = CreateTestEvent(evId, totalSeats);


            var b1 = await _bookingService.CreateBookingAsync(evId);
            var b2 = await _bookingService.CreateBookingAsync(evId);
            Assert.Equal(0, @event.AvailableSeats);


            await Assert.ThrowsAsync<NoAvailableSeatsException>(
                () => _bookingService.CreateBookingAsync(evId));


            await _bookingService.RejectBookingAsync(b1.Id);
            ReleaseSeatsInDb(evId, 1);
            var modelAfterRelease = _context.Events.First(e => e.Id == evId);
            Assert.Equal(1, modelAfterRelease.AvailableSeats);


            var b3 = await _bookingService.CreateBookingAsync(evId);
            Assert.NotNull(b3);
            Assert.Equal(0, @event.AvailableSeats);
        }

        #endregion

        #region Status Transition Tests

        [Fact]
        public async Task ConfirmBooking_FillsProcessedAt()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId);
            Assert.Null(booking.ProcessedAt);

            await _bookingService.ConfirmBookingAsync(booking.Id);

            var confirmed = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.NotNull(confirmed.ProcessedAt);
            Assert.Equal(BookingStatus.Confirmed, confirmed.Status);
        }

        [Fact]
        public async Task RejectBooking_FillsProcessedAt()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId);
            Assert.Null(booking.ProcessedAt);

            await _bookingService.RejectBookingAsync(booking.Id);

            var rejected = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.NotNull(rejected.ProcessedAt);
            Assert.Equal(BookingStatus.Rejected, rejected.Status);
        }

        [Fact]
        public async Task RejectBooking_ReleaseSeats_EnablesNewBooking()
        {
            var evId = Guid.NewGuid();
            var @event = CreateTestEvent(evId, 1);

            var booking = await _bookingService.CreateBookingAsync(evId);

            // После резерва проверяем модель в БД
            var modelAfterBooking = _context.Events.First(e => e.Id == evId);
            Assert.Equal(0, modelAfterBooking.AvailableSeats);

            await _bookingService.RejectBookingAsync(booking.Id);
            // освобождаем места через вспомогательную функцию для БД
            ReleaseSeatsInDb(evId, 1);
            var modelAfterRelease = _context.Events.First(e => e.Id == evId);
            Assert.Equal(1, modelAfterRelease.AvailableSeats);

            var newBooking = await _bookingService.CreateBookingAsync(evId);
            Assert.NotNull(newBooking);
            Assert.NotEqual(booking.Id, newBooking.Id);
        }

        #endregion

        #region Concurrency Tests

        [Fact(Skip = "InMemory provider does not model real DB concurrency; skip concurrency test")]
        public async Task ConcurrentBookingRequests_ProtectsAgainstOverbooking()
        {
            var evId = Guid.NewGuid();
            int totalSeats = 5;
            int requestCount = 20;

            var @event = CreateTestEvent(evId, totalSeats);

            var tasks = new List<Task<(bool Success, Guid? BookingId)>>();

            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    try
                    {
                        var booking = await bookingService.CreateBookingAsync(evId);
                        return (Success: true, BookingId: (Guid?)booking.Id);
                    }
                    catch (NoAvailableSeatsException)
                    {
                        return (Success: false, BookingId: (Guid?)null);
                    }
                }));
            }

            var results = await Task.WhenAll(tasks);

            int successCount = results.Count(r => r.Success);
            int failureCount = results.Count(r => !r.Success);

            Assert.Equal(totalSeats, successCount);
            Assert.Equal(requestCount - totalSeats, failureCount);


            var modelAfter = _context.Events.First(e => e.Id == evId);
            Assert.Equal(0, modelAfter.AvailableSeats);
        }

        [Fact(Skip = "InMemory provider does not model real DB concurrency; skip concurrency test")]
        public async Task ConcurrentBookingRequests_EnsureUniqueIds()
        {
            var evId = Guid.NewGuid();
            int requestCount = 10;

            CreateTestEvent(evId, requestCount);

            var tasks = new List<Task<DomainBooking>>();

            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    return await bookingService.CreateBookingAsync(evId);
                }));
            }

            var bookings = await Task.WhenAll(tasks);


            Assert.Equal(requestCount, bookings.Length);


            var uniqueIds = new HashSet<Guid>(bookings.Select(b => b.Id));
            Assert.Equal(requestCount, uniqueIds.Count);

            Assert.All(bookings, b => Assert.Equal(BookingStatus.Pending, b.Status));

            Assert.All(bookings, b => Assert.Equal(evId, b.EventId));
        }

        [Fact(Skip = "InMemory provider does not model real DB concurrency; skip concurrency test")]
        public async Task ConcurrentBookings_WithMultipleSeatsPerBooking()
        {
            var evId = Guid.NewGuid();
            int totalSeats = 15;

            var @event = CreateTestEvent(evId, totalSeats);

            var tasks = new List<Task<(bool Success, int ReservedSeats)>>();


            for (int i = 0; i < 3; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    try
                    {
                        var booking = await bookingService.CreateBookingAsync(evId);

                        return (true, 1);
                    }
                    catch (NoAvailableSeatsException)
                    {
                        return (false, 0);
                    }
                }));
            }

            var results = await Task.WhenAll(tasks);

            int totalReserved = results.Where(r => r.Success).Sum(r => r.ReservedSeats);
            Assert.Equal(3, totalReserved);
            Assert.Equal(totalSeats - 3, @event.AvailableSeats);
        }

        [Fact(Skip = "InMemory provider does not model real DB concurrency; skip concurrency test")]
        public async Task HighVolumeBookingTest_1000ConcurrentRequests()
        {
            var evId = Guid.NewGuid();
            int totalSeats = 100;
            int requestCount = 1000;

            CreateTestEvent(evId, totalSeats);

            var tasks = new List<Task<(bool Success, Guid? BookingId)>>();

            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    try
                    {
                        var booking = await bookingService.CreateBookingAsync(evId);
                        return (Success: true, BookingId: (Guid?)booking.Id);
                    }
                    catch (NoAvailableSeatsException)
                    {
                        return (Success: false, BookingId: (Guid?)null);
                    }
                }));
            }

            var results = await Task.WhenAll(tasks);

            int successCount = results.Count(r => r.Success);
            int failureCount = results.Count(r => !r.Success);

            Assert.Equal(totalSeats, successCount);
            Assert.Equal(requestCount - totalSeats, failureCount);


            var successfulIds = results.Where(r => r.Success).Select(r => r.BookingId).ToHashSet();
            Assert.Equal(totalSeats, successfulIds.Count);
        }

        #endregion
    }
}

