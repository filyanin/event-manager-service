namespace EventService.Domain.Filters;

public record EventsFilters
{
    public string? Title { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public EventsFilters(string? title, DateTime? from, DateTime? to)
    {
        if (from != null && to != null && from > to)
        {
            throw new ArgumentException("From must be earlier or equal To");
        }

        Title = title;
        From = from;
        To = to;
    }
}
