using BookingService.Domain.Enum;

namespace BookingService.Application.DTOs;

public class BookingDTO
{
    public Guid Id { get; set; }
    public Guid EventGuid { get; set; }
    public Guid UserGuid { get; set; }
    public int SeatsBooked { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class CreateBookingRequest
{
    public Guid EventId { get; set; }
    public int SeatsToBook { get; set; } = 1;
}

public class BookingResponse
{
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
    public BookingStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
}
