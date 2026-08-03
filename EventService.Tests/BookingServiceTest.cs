using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Exceptions;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventManagerService.Domain.Models;
using EventManagerService.Application.Interfaces;
using EventManagerService.Application.DTOs;
using EventManagerService.Infrastructure;
using EventManagerService.Application;
using EventManagerService.Shared.Exceptions;
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

            // Создаем конфигурацию для тестов
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "JwtSettings:Secret", "test-secret-key-at-least-32-characters-for-testing" },
                    { "JwtSettings:Issuer", "TestIssuer" },
                    { "JwtSettings:Audience", "TestAudience" },
                    { "JwtSettings:ExpirationMinutes", "60" }
                })
                .Build();

            services.AddInfrastructure(configuration);
            services.AddApplication();

            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<AppDbContext>();
            _eventService = _serviceProvider.GetRequiredService<IEventService>();
            _bookingService = _serviceProvider.GetRequiredService<IBookingService>();
        }

        private EventManagerService.Infrastructure.DataAssets.Models.Event CreateTestEvent(Guid eventId, int totalSeats = 100)
        {
            var domainEvent = DomainEvent.Create(eventId, "Test Event", DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2), totalSeats, totalSeats);

            // Создаём модель инфраструктуры напрямую из полей доменной сущности
            var model = new EventManagerService.Infrastructure.DataAssets.Models.Event
            {
                Id = domainEvent.Id,
                Title = domainEvent.Title,
                Description = domainEvent.Description,
                StartAt = domainEvent.StartAt,
                EndAt = domainEvent.EndAt,
                TotalSeats = domainEvent.TotalSeats,
                AvailableSeats = domainEvent.AvailableSeats
            };

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
            var booking = await bookingService.CreateBookingAsync(evId, Guid.NewGuid());

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
            await Assert.ThrowsAsync<AppException>(() => _bookingService.CreateBookingAsync(evId, Guid.NewGuid()));
        }

        [Fact]
        public async Task ConfirmBooking_ExistingBooking_ChangesStatusToConfirmed()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());

            await _bookingService.ConfirmBookingAsync(booking.Id);

            var fetched = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Confirmed, fetched.Status);
        }

        [Fact]
        public async Task RejectBooking_ExistingBooking_ChangesStatusToRejected()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());

            await _bookingService.RejectBookingAsync(booking.Id);

            var fetched = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Rejected, fetched.Status);

        }

        [Fact]
        public async Task ConfirmBooking_NonExistingBooking_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsAsync<AppException>(() => _bookingService.ConfirmBookingAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateMultipleBookings_SameEvent_UniqueIds()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var b1 = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());
            var b2 = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());

            Assert.NotEqual(b1.Id, b2.Id);
            Assert.Equal(evId, b1.EventId);
            Assert.Equal(evId, b2.EventId);
        }

        [Fact]
        public async Task CreateBooking_EventDeletedBetweenCalls_SecondCreateThrows()
        {
            throw new NotImplementedException();
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var first = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());
            Assert.NotNull(first);

            // Симулируем удаление события между вызовами
            var model = _context.Events.First(e => e.Id == evId);
            _context.Events.Remove(model);
            _context.SaveChanges();

            await Assert.ThrowsAsync<AppException>(() => _bookingService.CreateBookingAsync(evId, Guid.NewGuid()));
        }

        [Fact]
        public async Task GetBookingById_NonExistingId_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsAsync<AppException>(() => _bookingService.GetBookingByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetBookingByState_ReturnsOnlyRequestedState()
        {
            throw new NotImplementedException();
            var ev1 = Guid.NewGuid();
            var ev2 = Guid.NewGuid();
            CreateTestEvent(ev1);
            CreateTestEvent(ev2);

            var b1 = await _bookingService.CreateBookingAsync(ev1, Guid.NewGuid());
            var b2 = await _bookingService.CreateBookingAsync(ev2, Guid.NewGuid());

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
            throw new NotImplementedException();
            var evId = Guid.NewGuid();
            var @event = CreateTestEvent(evId, 100);
            int initialSeats = @event.AvailableSeats;

            var booking = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());

            Assert.Equal(initialSeats - 1, @event.AvailableSeats);
            Assert.NotNull(booking);
        }

        [Fact]
        public async Task CreateMultipleBookings_UpToLimit_AllSuccessful()
        {
            throw new NotImplementedException();
            var evId = Guid.NewGuid();
            int totalSeats = 5;
            var @event = CreateTestEvent(evId, totalSeats);

            var bookings = new List<BookingDTO>();
            for (int i = 0; i < totalSeats; i++)
            {
                var booking = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid() );
                bookings.Add(booking);
                Assert.Equal(totalSeats - (i + 1), @event.AvailableSeats);
            }

            Assert.Equal(totalSeats, bookings.Count);
            Assert.Equal(0, @event.AvailableSeats);

            // Verify all have unique IDs
            var uniqueIds = new HashSet<Guid>(bookings.Select(b => b.Id));
            Assert.Equal(bookings.Count, uniqueIds.Count);
        }

        [Fact]
        public async Task CreateBooking_ExhaustedSeats_ThrowsNoAvailableSeatsException()
        {
            throw new NotImplementedException();
            var evId = Guid.NewGuid();
            var @event = CreateTestEvent(evId, 1);

            // Create first booking - succeeds
            var booking1 = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());
            Assert.NotNull(booking1);
            Assert.Equal(0, @event.AvailableSeats);

            // Try to create second booking - should fail
            var exception = await Assert.ThrowsAsync<NoAvailableSeatsException>(
                () => _bookingService.CreateBookingAsync(evId, Guid.NewGuid()));
            Assert.Equal(EventManagerService.Shared.ErrorCodes.ErrorCodes.NoEnoughAvailableSeatsError, exception.Code);
        }

        [Fact]
        public async Task ReleaseSeats_RestoresAvailability()
        {
            throw new NotImplementedException();
            var evId = Guid.NewGuid();
            int totalSeats = 10;
            var @event = CreateTestEvent(evId, totalSeats);

            // Резервируем 3 места
            await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());
            await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());
            await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());
            Assert.Equal(totalSeats - 3, @event.AvailableSeats);

            // Освобождаем 2 места через вспомогательную функцию работы с БД
            ReleaseSeatsInDb(evId, 2);
            var modelAfterRelease = _context.Events.First(e => e.Id == evId);
            Assert.Equal(totalSeats - 1, modelAfterRelease.AvailableSeats);
        }


        #endregion

        #region Status Transition Tests

        [Fact]
        public async Task ConfirmBooking_FillsProcessedAt()
        {
            throw new NotImplementedException();
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());

            await _bookingService.ConfirmBookingAsync(booking.Id);

            var confirmed = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Confirmed, confirmed.Status);
        }

        [Fact]
        public async Task RejectBooking_FillsProcessedAt()
        {
            throw new NotImplementedException();
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());


            await _bookingService.RejectBookingAsync(booking.Id);

            var rejected = await _bookingService.GetBookingByIdAsync(booking.Id);

            Assert.Equal(BookingStatus.Rejected, rejected.Status);
        }

        [Fact]
        public async Task RejectBooking_ReleaseSeats_EnablesNewBooking()
        {
            throw new NotImplementedException();
            var evId = Guid.NewGuid();
            var @event = CreateTestEvent(evId, 1);

            var booking = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());

            // После резерва проверяем модель в БД
            var modelAfterBooking = _context.Events.First(e => e.Id == evId);
            Assert.Equal(0, modelAfterBooking.AvailableSeats);

            await _bookingService.RejectBookingAsync(booking.Id);
            // освобождаем места через вспомогательную функцию для БД
            ReleaseSeatsInDb(evId, 1);
            var modelAfterRelease = _context.Events.First(e => e.Id == evId);
            Assert.Equal(1, modelAfterRelease.AvailableSeats);

            var newBooking = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());
            Assert.NotNull(newBooking);
            Assert.NotEqual(booking.Id, newBooking.Id);
        }

        #endregion

       
    }
}

