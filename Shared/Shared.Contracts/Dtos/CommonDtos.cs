namespace Shared.Contracts.Dtos;

/// <summary>
/// DTO для данных пользователя, используемый при синхронной коммуникации между сервисами
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Role { get; set; }
}

/// <summary>
/// DTO для данных события, используемый при синхронной коммуникации между сервисами
/// </summary>
public class EventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO для данных бронирования, используемый при синхронной коммуникации между сервисами
/// </summary>
public class BookingDto
{
    public Guid Id { get; set; }
    public Guid UserGuid { get; set; }
    public Guid EventGuid { get; set; }
    public int SeatsBooked { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
