namespace EventManagerService.Domain.Filters
{
    public record EventsFilters
    {
        public string? Title {  get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public EventsFilters(string? title, DateTime? from, DateTime? to)
        {
            Title = title;
            From = from;
            To = to;
        }
    }


}
