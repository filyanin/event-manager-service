using EventManagerService.Properties;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Resources;

namespace EventManagerService.Domain.Models.Event
{
    public class Event
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

        private Event(string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = TotalSeats;
        }

        public Event(Guid id, string title, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats, string? description = null)
        {
            Id = id;
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = availableSeats;
        }
        public static Event Create(string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            
            if (title.Length < _minTitleLength || title.Length > _maxTitleLength)
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("StringLengthError"), nameof(title), _minTitleLength,_maxTitleLength));
            
            if (startAt >= endAt)
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"), nameof(endAt), nameof(startAt)));
            //ArgumentException используется намерено, для сохранения единого вида обработки ошибки некорректного аргумента
            if (totalSeats <= 0)
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("TotalSeatsValidationError"), nameof(totalSeats)));
                
            return new Event(title, startAt,endAt, totalSeats,description); ;


        }

        public void UpdateEvent(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            if (title.Length < _minTitleLength || title.Length > _maxTitleLength)
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("StringLengthError"), nameof(title), _minTitleLength, _maxTitleLength));

            if (startAt >= endAt)
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"), nameof(endAt), nameof(startAt)));

            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }

        public bool TryReserveSeats(int count = 1)
        {
            if (count <= 0)
            {
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"), nameof(count), "0"));
            }

            if (AvailableSeats - count < 0)
            {
                return false;
            }
            else
            {
                AvailableSeats -= count;
                return true;
            }
        }

        public bool ReleaseSeats(int count = 1)
        {
            if (count <= 0)
            {
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"), nameof(count), "0"));
            }

            if (AvailableSeats + count > TotalSeats)
            {
                return false;
            }

            AvailableSeats += count;
            return true;
        }

        public EventManagerService.Infrastructure.DataAssets.Models.Event ConvertTo()
        {
            return new EventManagerService.Infrastructure.DataAssets.Models.Event
            {
                Id = this.Id,
                Title = this.Title,
                Description = this.Description,
                StartAt = this.StartAt,
                EndAt = this.EndAt,
                TotalSeats = this.TotalSeats,
                AvailableSeats = this.AvailableSeats,
                Bookings = null
            };
        }
    }
}
