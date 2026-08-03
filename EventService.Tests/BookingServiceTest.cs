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
            var userId = Guid.NewGuid();
            CreateTestEvent(evId);

            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(evId, userId);

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
            var userId = Guid.NewGuid();
            await Assert.ThrowsAsync<AppException>(() => _bookingService.CreateBookingAsync(evId, userId));
        }

        [Fact]
        public async Task ConfirmBooking_ExistingBooking_ChangesStatusToConfirmed()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);

            await _bookingService.ConfirmBookingAsync(booking.Id);

            var fetched = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Confirmed, fetched.Status);
        }

        [Fact]
        public async Task RejectBooking_ExistingBooking_ChangesStatusToRejected()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);

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
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            CreateTestEvent(evId);

            var b1 = await _bookingService.CreateBookingAsync(evId, userId1);
            var b2 = await _bookingService.CreateBookingAsync(evId, userId2);

            Assert.NotEqual(b1.Id, b2.Id);
            Assert.Equal(evId, b1.EventId);
            Assert.Equal(evId, b2.EventId);
        }

        [Fact]
        public async Task CreateBooking_EventDeletedBetweenCalls_SecondCreateThrows()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            CreateTestEvent(evId);

            var first = await _bookingService.CreateBookingAsync(evId, userId);
            Assert.NotNull(first);

            // Симулируем удаление события между вызовами
            var model = _context.Events.First(e => e.Id == evId);
            _context.Events.Remove(model);
            _context.SaveChanges();

            await Assert.ThrowsAsync<AppException>(() => _bookingService.CreateBookingAsync(evId, userId));
        }

        [Fact]
        public async Task GetBookingById_NonExistingId_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsAsync<AppException>(() => _bookingService.GetBookingByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetBookingByState_ReturnsOnlyRequestedState()
        {
            var ev1 = Guid.NewGuid();
            var ev2 = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            CreateTestEvent(ev1);
            CreateTestEvent(ev2);

            var b1 = await _bookingService.CreateBookingAsync(ev1, userId1);
            var b2 = await _bookingService.CreateBookingAsync(ev2, userId2);

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
            var userId = Guid.NewGuid();
            var @event = CreateTestEvent(evId, 100);
            int initialSeats = @event.AvailableSeats;

            var booking = await _bookingService.CreateBookingAsync(evId, userId);

            // Получаем обновленное событие из БД
            var updatedEvent = _context.Events.First(e => e.Id == evId);
            Assert.Equal(initialSeats - 1, updatedEvent.AvailableSeats);
            Assert.NotNull(booking);
        }

        [Fact]
        public async Task CreateMultipleBookings_UpToLimit_AllSuccessful()
        {
            var evId = Guid.NewGuid();
            int totalSeats = 5;
            var @event = CreateTestEvent(evId, totalSeats);

            var bookings = new List<BookingDTO>();
            for (int i = 0; i < totalSeats; i++)
            {
                var booking = await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());
                bookings.Add(booking);

                // Получаем обновленное событие для проверки
                var updatedEvent = _context.Events.First(e => e.Id == evId);
                Assert.Equal(totalSeats - (i + 1), updatedEvent.AvailableSeats);
            }

            Assert.Equal(totalSeats, bookings.Count);

            // Verify all have unique IDs
            var uniqueIds = new HashSet<Guid>(bookings.Select(b => b.Id));
            Assert.Equal(bookings.Count, uniqueIds.Count);
        }

        [Fact]
        public async Task CreateBooking_ExhaustedSeats_ThrowsNoAvailableSeatsException()
        {
            var evId = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var @event = CreateTestEvent(evId, 1);

            // Create first booking - succeeds
            var booking1 = await _bookingService.CreateBookingAsync(evId, userId1);
            Assert.NotNull(booking1);

            var updatedEvent = _context.Events.First(e => e.Id == evId);
            Assert.Equal(0, updatedEvent.AvailableSeats);

            // Try to create second booking - should fail
            var exception = await Assert.ThrowsAsync<NoAvailableSeatsException>(
                () => _bookingService.CreateBookingAsync(evId, userId2));
            Assert.Equal(EventManagerService.Shared.ErrorCodes.ErrorCodes.NoEnoughAvailableSeatsError, exception.Code);
        }

        [Fact]
        public async Task ReleaseSeats_RestoresAvailability()
        {
            var evId = Guid.NewGuid();
            int totalSeats = 10;
            var @event = CreateTestEvent(evId, totalSeats);

            // Резервируем 3 места от разных пользователей
            await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());
            await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());
            await _bookingService.CreateBookingAsync(evId, Guid.NewGuid());

            var modelAfterBookings = _context.Events.First(e => e.Id == evId);
            Assert.Equal(totalSeats - 3, modelAfterBookings.AvailableSeats);

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
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);

            await _bookingService.ConfirmBookingAsync(booking.Id);

            var confirmed = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Confirmed, confirmed.Status);
        }

        [Fact]
        public async Task RejectBooking_FillsProcessedAt()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);

            await _bookingService.RejectBookingAsync(booking.Id);

            var rejected = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Rejected, rejected.Status);
        }

        [Fact]
        public async Task RejectBooking_ReleaseSeats_EnablesNewBooking()
        {
            var evId = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var @event = CreateTestEvent(evId, 1);

            var booking = await _bookingService.CreateBookingAsync(evId, userId1);

            // После резерва проверяем модель в БД
            var modelAfterBooking = _context.Events.First(e => e.Id == evId);
            Assert.Equal(0, modelAfterBooking.AvailableSeats);

            await _bookingService.RejectBookingAsync(booking.Id);

            // освобождаем места через вспомогательную функцию для БД
            ReleaseSeatsInDb(evId, 1);
            var modelAfterRelease = _context.Events.First(e => e.Id == evId);
            Assert.Equal(1, modelAfterRelease.AvailableSeats);

            var newBooking = await _bookingService.CreateBookingAsync(evId, userId2);
            Assert.NotNull(newBooking);
            Assert.NotEqual(booking.Id, newBooking.Id);
        }

        #endregion

        #region Past Event Booking Tests

        [Fact]
        public async Task CreateBooking_PastEvent_ThrowsPastEventBookingException()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Создаем событие, которое уже прошло
            var domainEvent = DomainEvent.Create(evId, "Past Event", DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1), 100, 100);
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

            // Попытка забронировать прошедшее событие должна выбросить исключение
            var exception = await Assert.ThrowsAsync<PastEventBookingException>(
                () => _bookingService.CreateBookingAsync(evId, userId));
            Assert.Equal(EventManagerService.Shared.ErrorCodes.ErrorCodes.PastEventBookingError, exception.Code);
        }

        #endregion

        #region Active Bookings Limit Tests

        [Fact]
        public async Task CreateBooking_LimitNotReached_SuccessfulBooking()
        {
            var userId = Guid.NewGuid();
            const int testLimit = 3; // Меньше чем реальный лимит 10

            // Создаём несколько событий
            var eventIds = new List<Guid>();
            for (int i = 0; i < testLimit; i++)
            {
                var eventId = Guid.NewGuid();
                eventIds.Add(eventId);
                CreateTestEvent(eventId, 100);
            }

            // Создаём бронирования для одного пользователя
            for (int i = 0; i < testLimit; i++)
            {
                var booking = await _bookingService.CreateBookingAsync(eventIds[i], userId);
                Assert.NotNull(booking);
                Assert.Equal(eventIds[i], booking.EventId);
            }
        }

        [Fact]
        public async Task CreateBooking_MaxLimitExceeded_ThrowsActiveBookingsLimitException()
        {
            var userId = Guid.NewGuid();
            const int maxActiveBookings = 10;

            // Создаём 10 событий и бронируем их один пользователем
            var eventIds = new List<Guid>();
            for (int i = 0; i < maxActiveBookings; i++)
            {
                var eventId = Guid.NewGuid();
                eventIds.Add(eventId);
                CreateTestEvent(eventId, 100);

                var booking = await _bookingService.CreateBookingAsync(eventId, userId);
                Assert.NotNull(booking);
            }

            // Создаём ещё одно событие для попытки превышения лимита
            var extraEventId = Guid.NewGuid();
            CreateTestEvent(extraEventId, 100);

            // Попытка создать 11-ю бронь должна выбросить исключение
            var exception = await Assert.ThrowsAsync<ActiveBookingsLimitException>(
                () => _bookingService.CreateBookingAsync(extraEventId, userId));
            Assert.Equal(EventManagerService.Shared.ErrorCodes.ErrorCodes.ActiveBookingsLimitExceededError, exception.Code);
            Assert.Equal(maxActiveBookings, exception.CurrentCount);
        }

        [Fact]
        public async Task CreateBooking_DifferentUsersIndependentLimits()
        {
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();
            const int maxActiveBookings = 10;

            // Создаём 10 событий для первого пользователя
            var eventIds = new List<Guid>();
            for (int i = 0; i < maxActiveBookings; i++)
            {
                var eventId = Guid.NewGuid();
                eventIds.Add(eventId);
                CreateTestEvent(eventId, 100);

                var booking = await _bookingService.CreateBookingAsync(eventId, user1);
                Assert.NotNull(booking);
            }

            // Второй пользователь должен иметь собственный лимит
            var user2EventIds = new List<Guid>();
            for (int i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                user2EventIds.Add(eventId);
                CreateTestEvent(eventId, 100);

                var booking = await _bookingService.CreateBookingAsync(eventId, user2);
                Assert.NotNull(booking);
                Assert.Equal(eventId, booking.EventId);
            }

            // Второй пользователь все ещё может добавить ещё бронирования
            var anotherEventId = Guid.NewGuid();
            CreateTestEvent(anotherEventId, 100);
            var anotherBooking = await _bookingService.CreateBookingAsync(anotherEventId, user2);
            Assert.NotNull(anotherBooking);
        }

        [Fact]
        public async Task CreateBooking_AfterRejectingBooking_DecreasesActiveCount()
        {
            var userId = Guid.NewGuid();
            const int maxActiveBookings = 10;

            // Создаём ровно 10 активных броней
            var eventIds = new List<Guid>();
            var bookingIds = new List<Guid>();
            for (int i = 0; i < maxActiveBookings; i++)
            {
                var eventId = Guid.NewGuid();
                eventIds.Add(eventId);
                CreateTestEvent(eventId, 100);

                var booking = await _bookingService.CreateBookingAsync(eventId, userId);
                bookingIds.Add(booking.Id);
            }

            // Отклоняем одну бронь
            await _bookingService.RejectBookingAsync(bookingIds[0]);

            // Теперь должны быть 9 активных броней, и новая бронь должна пройти
            var newEventId = Guid.NewGuid();
            CreateTestEvent(newEventId, 100);

            var newBooking = await _bookingService.CreateBookingAsync(newEventId, userId);
            Assert.NotNull(newBooking);
        }

        #endregion

        #region Booking Cancellation Tests

        [Fact]
        public async Task CancelBooking_PendingBooking_SuccessfullyCancelled()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            CreateTestEvent(evId, 10);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);
            Assert.Equal(BookingStatus.Pending, booking.Status);

            // Отмена бронирования
            await _bookingService.CancelBookingAsync(booking.Id, userId);

            var cancelledBooking = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Cancelled, cancelledBooking.Status);
        }

        [Fact]
        public async Task CancelBooking_ConfirmedBooking_SuccessfullyCancelled()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            CreateTestEvent(evId, 10);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);
            await _bookingService.ConfirmBookingAsync(booking.Id);

            var confirmedBooking = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Confirmed, confirmedBooking.Status);

            // Отмена подтвержденной бронирования
            await _bookingService.CancelBookingAsync(booking.Id, userId);

            var cancelledBooking = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Cancelled, cancelledBooking.Status);
        }

        [Fact]
        public async Task CancelBooking_ReleasesSeats()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var @event = CreateTestEvent(evId, 5);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);
            var eventAfterBooking = _context.Events.First(e => e.Id == evId);
            Assert.Equal(4, eventAfterBooking.AvailableSeats);

            // Отмена бронирования должна освободить место
            await _bookingService.CancelBookingAsync(booking.Id, userId);

            var eventAfterCancellation = _context.Events.First(e => e.Id == evId);
            Assert.Equal(5, eventAfterCancellation.AvailableSeats);
        }

        [Fact]
        public async Task CancelBooking_DoubleCancellation_ThrowsException()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            CreateTestEvent(evId, 10);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);

            // Первая отмена - успешна
            await _bookingService.CancelBookingAsync(booking.Id, userId);

            // Вторая попытка отмены должна выбросить исключение
            // (защита от повторной отмены - бронирование уже в статусе Cancelled)
            await Assert.ThrowsAsync<EventManagerService.Domain.Exceptions.GreaterThenValidationException>(
                () => _bookingService.CancelBookingAsync(booking.Id, userId));
        }

        [Fact]
        public async Task CancelBooking_RejectedBooking_ThrowsException()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            CreateTestEvent(evId, 10);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);

            // Отклонение бронирования
            await _bookingService.RejectBookingAsync(booking.Id);

            // Попытка отмены отклоненной бронирования должна выбросить исключение
            // (отклоненная бронирование не может быть отменена)
            await Assert.ThrowsAsync<EventManagerService.Domain.Exceptions.GreaterThenValidationException>(
                () => _bookingService.CancelBookingAsync(booking.Id, userId));
        }

        [Fact]
        public async Task CancelBooking_NonExistingBooking_ThrowsNotFoundException()
        {
            var userId = Guid.NewGuid();
            var fakeBookingId = Guid.NewGuid();

            await Assert.ThrowsAsync<AppException>(
                () => _bookingService.CancelBookingAsync(fakeBookingId, userId));
        }

        [Fact]
        public async Task CancelBooking_UserCancelOwnBooking_SuccessfullyCancelled()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            CreateTestEvent(evId, 10);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);

            // Пользователь отменяет свою бронь
            await _bookingService.CancelBookingAsync(booking.Id, userId, "user");

            var cancelledBooking = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Cancelled, cancelledBooking.Status);
        }

        [Fact]
        public async Task CancelBooking_UserCancelOtherUserBooking_ThrowsUnauthorizedException()
        {
            var evId = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            CreateTestEvent(evId, 10);

            var booking = await _bookingService.CreateBookingAsync(evId, userId1);

            // Другой пользователь пытается отменить чужую бронь
            await Assert.ThrowsAsync<UnauthorizedBookingCancellationException>(
                () => _bookingService.CancelBookingAsync(booking.Id, userId2, "user"));

            // Бронирование должно остаться в статусе Pending
            var bookingAfter = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Pending, bookingAfter.Status);
        }

        [Fact]
        public async Task CancelBooking_AdminCancelOtherUserBooking_SuccessfullyCancelled()
        {
            var evId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            CreateTestEvent(evId, 10);

            var booking = await _bookingService.CreateBookingAsync(evId, userId);

            // Администратор отменяет чужую бронь
            await _bookingService.CancelBookingAsync(booking.Id, adminId, "admin");

            var cancelledBooking = await _bookingService.GetBookingByIdAsync(booking.Id);
            Assert.Equal(BookingStatus.Cancelled, cancelledBooking.Status);
        }

        [Fact]
        public async Task CancelBooking_DecreasesActiveBookingCount()
        {
            var userId = Guid.NewGuid();
            const int testLimit = 3;

            var eventIds = new List<Guid>();
            var bookingIds = new List<Guid>();
            for (int i = 0; i < testLimit; i++)
            {
                var eventId = Guid.NewGuid();
                eventIds.Add(eventId);
                CreateTestEvent(eventId, 100);

                var booking = await _bookingService.CreateBookingAsync(eventId, userId);
                bookingIds.Add(booking.Id);
            }

            // У пользователя есть 3 активные брони
            // Отменяем одну
            await _bookingService.CancelBookingAsync(bookingIds[0], userId);

            // Теперь должно быть 2 активные брони
            // Создаем еще одну бронь для проверки (должна пройти успешно)
            var newEventId = Guid.NewGuid();
            CreateTestEvent(newEventId, 100);

            var newBooking = await _bookingService.CreateBookingAsync(newEventId, userId);
            Assert.NotNull(newBooking);
        }

        #endregion
    }
}

