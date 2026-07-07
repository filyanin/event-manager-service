using EventManagerService.Domain.Enum;
using EventManagerService.Domain.Exceptions;
using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Models.DomainBooking;
using EventManagerService.Properties;
using System.Resources;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using EventManagerService.Domain.Interfaces.Repositories;

namespace EventManagerService.Domain.Services.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        private static readonly SemaphoreSlim _bookingSemaphore = new SemaphoreSlim(1, 1);

        public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        }

        public async Task<DomainBooking> CreateBookingAsync(Guid eventId)
        {
            await _bookingSemaphore.WaitAsync();
            try
            {
                // Проверяем существование события и резервируем место
                if (!await _eventRepository.ExistsAsync(eventId))
                {
                    throw new KeyNotFoundException(string.Format(
                        ErrorMessages.ObjectNotFound, eventId));
                }

                var reserved = await _eventRepository.TryReserveSeatsAsync(eventId, 1);
                if (!reserved)
                {
                    throw new NoAvailableSeatsException();
                }

                // Создаём запись брони в репозитории
                return await _bookingRepository.CreateAsync(eventId);
            }
            finally
            {
                _bookingSemaphore.Release();
            }
        }

        public async Task<DomainBooking> GetBookingByIdAsync(Guid bookingId)
        {
            return await _bookingRepository.GetByIdAsync(bookingId);
        }

        public async Task<List<DomainBooking>> GetBookingByStateAsync(BookingStatus state)
        {
            return await _bookingRepository.GetByStateAsync(state);
        }

        public async Task ConfirmBookingAsync(Guid bookingId)
        {
            await _bookingRepository.ConfirmAsync(bookingId);
        }

        public async Task RejectBookingAsync(Guid bookingId)
        {
            await _bookingRepository.RejectAsync(bookingId);
        }
    }
}
