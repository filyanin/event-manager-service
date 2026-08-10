namespace EventManagerService.Domain.Exceptions
{
    public class ActiveBookingsLimitException : DomainValidationException
    {
        public string Code { get; }

        public int CurrentCount { get; }
        public int MaxLimit { get; }

        public ActiveBookingsLimitException(string code, int currentCount, int maxLimit) : base(code)
        {
            Code = code;
            CurrentCount = currentCount;
            MaxLimit = maxLimit;

            Data["currentCount"] = currentCount;
            Data["maxLimit"] = maxLimit;
        }

        public ActiveBookingsLimitException(string code, int currentCount, int maxLimit, Exception innerException) : base(code, innerException)
        {
            Code = code;
            CurrentCount = currentCount;
            MaxLimit = maxLimit;

            Data["currentCount"] = currentCount;
            Data["maxLimit"] = maxLimit;
        }
    }
}
