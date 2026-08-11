using BookingService.Application.DTOs;
using BookingService.Application.Interfaces;
using BookingService.Domain.Enum;
using BookingService.Domain.Models;
using Shared.Contracts.Events.Booking;
using Shared.Contracts.Topics;
using BookingService.Infrastructure.Kafka;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<BookingService> _logger;
    private const int MaxActiveBookings = 10;

    public BookingService(IBookingRepository bookingRepository, IKafkaProducer kafkaProducer, ILogger<BookingService> logger)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BookingDTO> CreateBookingAsync(Guid eventGuid, Guid userGuid, int seatsToBook = 1)
    {
        if (seatsToBook <= 0)
            throw new ArgumentException("SeatsToBook must be greater than 0", nameof(seatsToBook));

        // Проверяем количество активных бронирований пользователя
        var activeBookingsCount = await _bookingRepository.GetActiveBookingCountAsync(userGuid);
        if (activeBookingsCount >= MaxActiveBookings)
            throw new InvalidOperationException($"User has reached maximum active bookings limit ({MaxActiveBookings})");

        var booking = new DomainBooking(eventGuid, userGuid, seatsToBook);

        var result = await _bookingRepository.CreateAsync(booking);

        return MapToDTO(result);
    }

    public async Task<BookingDTO> GetBookingByIdAsync(Guid bookingId)
    {
        var result = await _bookingRepository.GetByIdAsync(bookingId);
        return MapToDTO(result);
    }

    public async Task<List<BookingDTO>> GetBookingsByEventAsync(Guid eventGuid)
    {
        var results = await _bookingRepository.GetByEventGuidAsync(eventGuid);
        return results.Select(MapToDTO).ToList();
    }

    public async Task<List<BookingDTO>> GetBookingsByUserAsync(Guid userGuid)
    {
        var results = await _bookingRepository.GetByUserGuidAsync(userGuid);
        return results.Select(MapToDTO).ToList();
    }

    public async Task<List<BookingDTO>> GetBookingByStatusAsync(BookingStatus status)
    {
        var results = await _bookingRepository.GetByStatusAsync(status);
        return results.Select(MapToDTO).ToList();
    }

    public async Task ConfirmBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);

        if (booking == null)
            throw new KeyNotFoundException("Booking not found");

        booking.SetBookingConfirmed(DateTime.UtcNow);
        await _bookingRepository.ChangeBookingStateAsync(booking);

        // Публикуем событие в Kafka
        try
        {
            var @event = new BookingConfirmedEvent(booking.Id, booking.EventGuid, booking.SeatsBooked);
            await _kafkaProducer.PublishAsync(KafkaTopics.BookingConfirmed, booking.Id.ToString(), @event);
            _logger.LogInformation($"BookingConfirmed event published for booking {{{booking.Id}}}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to publish BookingConfirmed event: {ex.Message}");
            throw;
        }
    }

    public async Task RejectBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);

        if (booking == null)
            throw new KeyNotFoundException("Booking not found");

        booking.SetBookingRejected(DateTime.UtcNow);
        await _bookingRepository.ChangeBookingStateAsync(booking);
    }    public async Task CancelBookingAsync(Guid bookingId, Guid userGuid)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);

        if (booking == null)
            throw new KeyNotFoundException("Booking not found");

        if (booking.UserGuid != userGuid)
            throw new UnauthorizedAccessException("User is not authorized to cancel this booking");

        booking.SetBookingCancelled(DateTime.UtcNow);
        await _bookingRepository.ChangeBookingStateAsync(booking);
    }

    public async Task<int> GetActiveBookingsCountAsync(Guid userGuid)
    {
        return await _bookingRepository.GetActiveBookingCountAsync(userGuid);
    }

    private static BookingDTO MapToDTO(DomainBooking booking)
    {
        return new BookingDTO
        {
            Id = booking.Id,
            EventGuid = booking.EventGuid,
            UserGuid = booking.UserGuid,
            SeatsBooked = booking.SeatsBooked,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt
        };
    }
}
