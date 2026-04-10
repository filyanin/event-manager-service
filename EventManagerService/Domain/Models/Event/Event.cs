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

        private Event(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }
        public static Event Create(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            
            if (title.Length < _minTitleLength || title.Length > _maxTitleLength)
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("StringLengthError"), nameof(title), _minTitleLength,_maxTitleLength));
            
            if (startAt >= endAt)
                throw new ArgumentException(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"), nameof(endAt), nameof(startAt)));
                
            return new Event(title, startAt,endAt,description); ;


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
    }
}
