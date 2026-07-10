
namespace EventManagerService.Domain.Exceptions
{
    public class NoAvailableSeatsException : Exception
    {
        public string Code { get; }
        public NoAvailableSeatsException(string code) : base(code)
        {
            Code = code;
        }

        public NoAvailableSeatsException(string code, Exception innerException) : base(code, innerException)
        {
            Code = code;
        }
    }
}
