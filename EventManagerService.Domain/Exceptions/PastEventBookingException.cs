namespace EventManagerService.Domain.Exceptions
{
    public class PastEventBookingException : DomainValidationException
    {
        public string Code { get; }

        public PastEventBookingException(string code) : base(code)
        {
            Code = code;
        }

        public PastEventBookingException(string code, Exception innerException) : base(code, innerException)
        {
            Code = code;
        }
    }
}
