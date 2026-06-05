namespace EventManagerService.Domain.Exceptions
{
    public class NoAvailableSeatsException : Exception
    {
        public NoAvailableSeatsException(string message) : base(message)
        {
        }

        public NoAvailableSeatsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
