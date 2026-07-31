
namespace EventManagerService.Domain.Exceptions
{
    public class NoAvailableSeatsException : DomainValidationException
    {
        public string Code { get; }

        public string? FirstParamName { get; }
        public object? FirstParamValue { get; }

        public string? SecondParamName { get; }
        public object? SecondParamValue { get; }

        public NoAvailableSeatsException(string code) : base(code)
        {
            Code = code;
        }

        public NoAvailableSeatsException(string code, Exception innerException) : base(code, innerException)
        {
            Code = code;
        }

        public NoAvailableSeatsException(string code, string firstParamName, object firstParamValue, string? secondParamName = null, object? secondParamValue = null)
            : base(code)
        {
            Code = code;

            FirstParamName = firstParamName;
            FirstParamValue = firstParamValue;

            SecondParamName = secondParamName;
            SecondParamValue = secondParamValue;

            Data["firstParamName"] = FirstParamName;
            Data["firstParamValue"] = FirstParamValue;
            if (SecondParamName != null)
            {
                Data["secondParamName"] = SecondParamName;
                Data["secondParamValue"] = SecondParamValue;
            }
        }
    }
}
