using EventManagerService.Properties;
using System.Resources;

namespace EventManagerService.Domain.Exceptions
{
    public class NoAvailableSeatsException : Exception
    {
        public NoAvailableSeatsException() : base(new ResourceManager(typeof(ErrorMessages)).GetString("NoAvailableSeatsError"))
        {
        }

        public NoAvailableSeatsException(Exception innerException) : base(new ResourceManager(typeof(ErrorMessages)).GetString("NoAvailableSeatsError"), innerException)
        {
        }
    }
}
