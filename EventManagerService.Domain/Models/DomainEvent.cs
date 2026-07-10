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

        private DomainEvent(Guid id, string title, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats, string? description = null)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = availableSeats;
        }

        public static DomainEvent Create(Guid id, string title, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats, string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            if (title.Length < _minTitleLength || title.Length > _maxTitleLength)
            {
                var ex = new DomainValidationException("LengthValidationError");
                ex.Data["firstParamName"] = nameof(title);
                ex.Data["firstParamValue"] = _minTitleLength;

                throw ex;
            }

            if (startAt >= endAt)
            {
                var ex = new DomainValidationException("GreaterThanValidationError");
                ex.Data["firstParamName"] = nameof(startAt);
                ex.Data["firstParamValue"] = startAt;

                throw ex;
            }

            if (totalSeats < 0)
            {
                var ex = new DomainValidationException("TotalSeatsValidationError");
                ex.Data["firstParamName"] = nameof(totalSeats);
                ex.Data["firstParamValue"] = totalSeats;

                throw ex;
            }
            return new DomainEvent(id, title, startAt, endAt, totalSeats, availableSeats, description); ;


        }

        public void UpdateEvent(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            if (title.Length < _minTitleLength || title.Length > _maxTitleLength)
            {
                var ex = new DomainValidationException("LengthValidationError");
                ex.Data["firstParamName"] = nameof(title);
                ex.Data["firstParamValue"] = _minTitleLength;

                throw ex;
            }

            if (startAt >= endAt)
            {
                var ex = new DomainValidationException("GreaterThanValidationError");
                ex.Data["firstParamName"] = nameof(startAt);
                ex.Data["firstParamValue"] = startAt;

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
                var ex = new NoAvailableSeatsException("NoAviableSeatsError");
                ex.Data["firstParamName"] = nameof(seatsToReserve);
                ex.Data["firstParamValue"] = seatsToReserve;

                throw ex;
            }

            if (AvailableSeats - seatsToReserve < 0)
            {
                var ex = new NoAvailableSeatsException("NoEnoughAvailableSeatsError");
                ex.Data["firstParamName"] = nameof(seatsToReserve);
                ex.Data["firstParamValue"] = seatsToReserve;
                ex.Data["secondParamName"] = nameof(AvailableSeats);
                ex.Data["secondParamValue"] = AvailableSeats;

                throw ex;
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
                var ex = new DomainValidationException("WrongReleaseSeatsCountError");
                ex.Data["firstParamName"] = nameof(seatsToRelease);
                ex.Data["firstParamValue"] = seatsToRelease;
                ex.Data["secondParamName"] = "0";
                ex.Data["secondParamValue"] = "0";

                throw ex;
            }

            if (AvailableSeats + seatsToRelease > TotalSeats)
            {
                var ex = new DomainValidationException("WrongReleaseSeatsCountError");
                ex.Data["firstParamName"] = nameof(seatsToRelease);
                ex.Data["firstParamValue"] = seatsToRelease;
                ex.Data["secondParamName"] = nameof(TotalSeats);
                ex.Data["secondParamValue"] = TotalSeats;

                throw ex;
            }

            AvailableSeats += seatsToRelease;
            return true;
        }
    }
}
