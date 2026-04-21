using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Models.Booking;
using EventManagerService.Domain.Services.BookingService;
using Moq;
using Xunit;

namespace EventService.Tests
{
    public class BookingServiceTest
    {
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly IBookingService _bookingService;

        public BookingServiceTest()
        {
            _eventServiceMock = new Mock<IEventService>();
            _bookingService = new BookingService(_eventServiceMock.Object);
        }

        [Fact]
        public async Task CreateBooking_EventExists_CreatesBooking()
        {
            var evId = Guid.NewGuid();
            _eventServiceMock.Setup(s => s.CheckEventById(evId)).ReturnsAsync(true);

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
            _eventServiceMock.Setup(s => s.CheckEventById(evId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(evId));
        }

        [Fact]
        public async Task ConfirmBooking_ExistingBooking_ChangesStatusToConfirmed()
        {
            var evId = Guid.NewGuid();
            _eventServiceMock.Setup(s => s.CheckEventById(evId)).ReturnsAsync(true);

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
            _eventServiceMock.Setup(s => s.CheckEventById(evId)).ReturnsAsync(true);

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
            _eventServiceMock.Setup(s => s.CheckEventById(evId)).ReturnsAsync(true);

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
            _eventServiceMock.SetupSequence(s => s.CheckEventById(evId)).ReturnsAsync(true).ReturnsAsync(false);

            var first = await _bookingService.CreateBookingAsync(evId);
            Assert.NotNull(first);

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
            _eventServiceMock.Setup(s => s.CheckEventById(It.IsAny<Guid>())).ReturnsAsync(true);

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
    }
}
