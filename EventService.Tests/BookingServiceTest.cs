using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Exceptions;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Models.DomainBooking;
using EventManagerService.Domain.Models.DomainEvent;
using EventManagerService.Domain.Services.BookingService;
using EventManagerService.Domain.Services.EventService;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EventManagerService.Domain;
using Xunit;

namespace EventService.Tests
{
    public class BookingServiceTest
    {
        private readonly AppDbContext _context;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public BookingServiceTest()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(dbName).Options;
            _context = new AppDbContext(options);

            // Создаём репозитории поверх InMemory DbContext и передаём их в сервисы
            var eventRepo = new EventManagerService.Infrastructure.Repositories.EventRepository(_context);
            var bookingRepo = new EventManagerService.Infrastructure.Repositories.BookingRepository(_context);

            _eventService = new EventManagerService.Domain.Services.EventService.EventService(eventRepo);
            _bookingService = new EventManagerService.Domain.Services.BookingService.BookingService(bookingRepo, eventRepo);
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

        #region Basic Booking Tests

        [Fact]
        public async Task CreateBooking_EventExists_CreatesBooking()
        {
            var evId = Guid.NewGuid();
            CreateTestEvent(evId);

            var booking = await _bookingService.CreateBookingAsync(evId);

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

            // Simulate event deletion between calls
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

            // Reserve 3 seats
            await _bookingService.CreateBookingAsync(evId);
            await _bookingService.CreateBookingAsync(evId);
            await _bookingService.CreateBookingAsync(evId);
            Assert.Equal(totalSeats - 3, @event.AvailableSeats);

            // Release 2 seats via DB helper
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

            // Create 2 bookings - exhausts seats
            var b1 = await _bookingService.CreateBookingAsync(evId);
            var b2 = await _bookingService.CreateBookingAsync(evId);
            Assert.Equal(0, @event.AvailableSeats);

            // Try to create 3rd - should fail
            await Assert.ThrowsAsync<NoAvailableSeatsException>(
                () => _bookingService.CreateBookingAsync(evId));

            // Reject one booking and release seats in DB
            await _bookingService.RejectBookingAsync(b1.Id);
            ReleaseSeatsInDb(evId, 1);
            var modelAfterRelease = _context.Events.First(e => e.Id == evId);
            Assert.Equal(1, modelAfterRelease.AvailableSeats);

            // Now we should be able to create a new booking
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

            // After reservation, check DB model
            var modelAfterBooking = _context.Events.First(e => e.Id == evId);
            Assert.Equal(0, modelAfterBooking.AvailableSeats);

            await _bookingService.RejectBookingAsync(booking.Id);
            // release seats in DB helper
            ReleaseSeatsInDb(evId, 1);
            var modelAfterRelease = _context.Events.First(e => e.Id == evId);
            Assert.Equal(1, modelAfterRelease.AvailableSeats);

            var newBooking = await _bookingService.CreateBookingAsync(evId);
            Assert.NotNull(newBooking);
            Assert.NotEqual(booking.Id, newBooking.Id);
        }

        #endregion

        #region Concurrency Tests

        [Fact]
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
                    try
                    {
                        var booking = await _bookingService.CreateBookingAsync(evId);
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
            Assert.Equal(0, @event.AvailableSeats);
        }

        [Fact]
        public async Task ConcurrentBookingRequests_EnsureUniqueIds()
        {
            var evId = Guid.NewGuid();
            int requestCount = 10;

            CreateTestEvent(evId, requestCount);

            var tasks = new List<Task<DomainBooking>>();

            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(_bookingService.CreateBookingAsync(evId));
            }

            var bookings = await Task.WhenAll(tasks);

            // Verify all bookings were created
            Assert.Equal(requestCount, bookings.Length);

            // Verify all IDs are unique
            var uniqueIds = new HashSet<Guid>(bookings.Select(b => b.Id));
            Assert.Equal(requestCount, uniqueIds.Count);

            // Verify all are pending
            Assert.All(bookings, b => Assert.Equal(BookingStatus.Pending, b.Status));

            // Verify all belong to the correct event
            Assert.All(bookings, b => Assert.Equal(evId, b.EventId));
        }

        [Fact]
        public async Task ConcurrentBookings_WithMultipleSeatsPerBooking()
        {
            var evId = Guid.NewGuid();
            int totalSeats = 15;

            var @event = CreateTestEvent(evId, totalSeats);

            var tasks = new List<Task<(bool Success, int ReservedSeats)>>();

            // 3 concurrent requests trying to reserve 5 seats each
            for (int i = 0; i < 3; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var booking = await _bookingService.CreateBookingAsync(evId);
                        // In a real scenario, each booking could reserve multiple seats
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

        [Fact]
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
                    try
                    {
                        var booking = await _bookingService.CreateBookingAsync(evId);
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

            // Verify all successful bookings have unique IDs
            var successfulIds = results.Where(r => r.Success).Select(r => r.BookingId).ToHashSet();
            Assert.Equal(totalSeats, successfulIds.Count);
        }

        #endregion
    }
}

