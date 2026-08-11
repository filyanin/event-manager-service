namespace EventService.Domain.Models
{
    public class DomainEvent
    {
        private const int _minTitleLength = 6;
        private const int _maxTitleLength = 1000;
        public Guid Id { get; private set; }

        public string Title { get; private set; }

        public string? Description { get; private set; }

        public DateTime StartAt { get; private set; }

        public DateTime EndAt { get; private set; }
        public int TotalSeats { get; private set; }
        public int AvailableSeats { get; private set; }

        public Guid CreatedByUserId { get; private set; }

        public byte[]? Timestamp { get; private set; }

        private DomainEvent(Guid id, string title, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats, Guid createdByUserId, string? description = null, byte[]? timestamp = null)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = availableSeats;
            CreatedByUserId = createdByUserId;
            Timestamp = timestamp;
        }

        public static DomainEvent Create(Guid id, string title, DateTime startAt, DateTime endAt, int totalSeats, Guid createdByUserId, string? description = null, byte[]? timestamp = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required", nameof(title));

            if (title.Length < _minTitleLength || title.Length > _maxTitleLength)
                throw new ArgumentException($"Title length must be between {_minTitleLength} and {_maxTitleLength}", nameof(title));

            if (startAt >= endAt)
                throw new ArgumentException("StartAt must be earlier than EndAt");

            if (totalSeats < 0)
                throw new ArgumentException("TotalSeats must be non-negative", nameof(totalSeats));

            if (createdByUserId == Guid.Empty)
                throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

            return new DomainEvent(id, title, startAt, endAt, totalSeats, totalSeats, createdByUserId, description, timestamp);
        }

        public void UpdateEvent(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required", nameof(title));

            if (title.Length < _minTitleLength || title.Length > _maxTitleLength)
                throw new ArgumentException($"Title length must be between {_minTitleLength} and {_maxTitleLength}", nameof(title));

            if (startAt >= endAt)
                throw new ArgumentException("StartAt must be earlier than EndAt");

            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }

        public bool TryReserveSeats(int seatsToReserve = 1)
        {
            if (seatsToReserve <= 0)
                throw new ArgumentException("seatsToReserve must be greater than zero", nameof(seatsToReserve));

            if (AvailableSeats - seatsToReserve < 0)
                throw new InvalidOperationException("Not enough available seats");

            AvailableSeats -= seatsToReserve;
            return true;
        }

        public bool ReleaseSeats(int seatsToRelease = 1)
        {
            if (seatsToRelease <= 0)
                throw new ArgumentException("seatsToRelease must be greater than zero", nameof(seatsToRelease));

            if (AvailableSeats + seatsToRelease > TotalSeats)
                throw new InvalidOperationException("Release would exceed total seats");

            AvailableSeats += seatsToRelease;
            return true;
        }
    }
}
