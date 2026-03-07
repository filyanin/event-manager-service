using System.Data.Common;

namespace EventManagerService.Domain.Models
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
                throw new ArgumentException($"Title must be at least {_minTitleLength} and smaller then {_maxTitleLength}");

            if (startAt >= endAt)
                throw new ArgumentException($"End date {endAt} must be latest then start date {startAt}");
                
            return new Event(title, startAt,endAt,description); ;


        }

        public void UpdateEvent(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            if (title.Length < _minTitleLength || title.Length > _maxTitleLength)
                throw new ArgumentException($"Title must be at least {_minTitleLength} and smaller then {_maxTitleLength}");

            if (startAt >= endAt)
                throw new ArgumentException($"End date {endAt} must be latest then start date {startAt}");

            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }
    }
}
