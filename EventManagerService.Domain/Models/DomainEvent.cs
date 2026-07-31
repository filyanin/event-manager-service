using EventManagerService.Domain.Exceptions;
using System.Resources;

namespace EventManagerService.Domain.Models
{
    public class DomainEvent
    {
        private const int _minTitleLength = 6;
        private const int _maxTitleLength = 1000;
        public Guid Id { get; private set; }

        public string Title { get; private set; }

        public string? Description { get; private set; }

        public  DateTime StartAt {  get; private set; }

        public DateTime EndAt { get; private set; }
        public int TotalSeats { get; private set; }
        public int AvailableSeats { get; private set; }

        public byte[]? Timestamp { get; private set; }

        private DomainEvent(Guid id, string title, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats, string? description = null, byte[]? timestamp = null)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = availableSeats;
            Timestamp = timestamp;
        }

        public static DomainEvent Create(Guid id, string title, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats, string? description = null, byte[]? timestamp = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            if (title.Length < _minTitleLength || title.Length > _maxTitleLength)
            {
                var ex = new StringLengthValidationException(EventManagerService.Shared.ErrorCodes.ErrorCodes.LengthValidationError,
                    nameof(_minTitleLength),
                    nameof(_maxTitleLength),
                    _minTitleLength,
                    _maxTitleLength);

                throw ex;
            }

            if (startAt >= endAt)
            {
                var ex = new GreaterThenValidationException(EventManagerService.Shared.ErrorCodes.ErrorCodes.GreaterThanValidationError,
                    nameof(endAt),
                    nameof(startAt),
                    endAt,
                    startAt);


                throw ex;
            }

            if (totalSeats < 0)
            {
                var ex = new GreaterThenValidationException(EventManagerService.Shared.ErrorCodes.ErrorCodes.GreaterThanValidationError,
                    nameof(totalSeats),
                    "zero",
                    totalSeats,
                    0);

                throw ex;
            }
            return new DomainEvent(id, title, startAt, endAt, totalSeats, availableSeats, description, timestamp);


        }

        public void UpdateEvent(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            if (title.Length < _minTitleLength || title.Length > _maxTitleLength)
            {
                var ex = new StringLengthValidationException(EventManagerService.Shared.ErrorCodes.ErrorCodes.LengthValidationError,
                    nameof(_minTitleLength),
                    nameof(_maxTitleLength),
                    _minTitleLength,
                    _maxTitleLength);

                throw ex;
            }

            if (startAt >= endAt)
            {
                var ex = new GreaterThenValidationException(EventManagerService.Shared.ErrorCodes.ErrorCodes.GreaterThanValidationError,
                    nameof(endAt),
                    nameof(startAt),
                    endAt,
                    startAt);

                throw ex;
            }

            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }

        public bool TryReserveSeats(int seatsToReserve = 1)
        {
            if (seatsToReserve <= 0)
            {
                // Структурированное исключение с параметрами
                throw new NoAvailableSeatsException(EventManagerService.Shared.ErrorCodes.ErrorCodes.NoAviableSeatsError, nameof(seatsToReserve), seatsToReserve);
            }

            if (AvailableSeats - seatsToReserve < 0)
            {
                throw new NoAvailableSeatsException(
                    EventManagerService.Shared.ErrorCodes.ErrorCodes.NoEnoughAvailableSeatsError,
                    nameof(seatsToReserve),
                    seatsToReserve,
                    nameof(AvailableSeats),
                    AvailableSeats);
            }
            else
            {
                AvailableSeats -= seatsToReserve;
                return true;
            }
        }

        public bool ReleaseSeats(int seatsToRelease = 1)
        {
            if (seatsToRelease <= 0)
            {
                throw new GreaterThenValidationException(
                    EventManagerService.Shared.ErrorCodes.ErrorCodes.WrongReleaseSeatsCountError,
                    nameof(seatsToRelease),
                    "0",
                    seatsToRelease,
                    0);
            }

            if (AvailableSeats + seatsToRelease > TotalSeats)
            {
                throw new GreaterThenValidationException(
                    EventManagerService.Shared.ErrorCodes.ErrorCodes.WrongReleaseSeatsCountError,
                    nameof(seatsToRelease),
                    nameof(TotalSeats),
                    seatsToRelease,
                    TotalSeats);
            }

            AvailableSeats += seatsToRelease;
            return true;
        }
    }
}
