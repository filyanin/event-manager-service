using EventManagerService.Domain.Exceptions;

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

            if (From != null && to != null && from > to)
            {
                var ex = new DomainValidationException("GreaterThanValidationError");
                ex.Data["FirstParamName"] = nameof(from);
                ex.Data["FirstParamValue"] = from;
                ex.Data["SecondParamName"] = nameof(to);
                ex.Data["SecondParamValue"] = to;

                throw ex;
            }

            From = from;
            To = to;
        }
    }


}
