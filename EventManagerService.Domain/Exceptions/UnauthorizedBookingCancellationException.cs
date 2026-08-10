namespace EventManagerService.Domain.Exceptions
{
    public class UnauthorizedBookingCancellationException : DomainValidationException
    {
        public string Code { get; }

        public Guid BookingId { get; }
        public Guid RequestingUserId { get; }

        public UnauthorizedBookingCancellationException(string code, Guid bookingId, Guid requestingUserId) : base(code)
        {
            Code = code;
            BookingId = bookingId;
            RequestingUserId = requestingUserId;

            Data["bookingId"] = bookingId;
            Data["requestingUserId"] = requestingUserId;
        }

        public UnauthorizedBookingCancellationException(string code, Guid bookingId, Guid requestingUserId, Exception innerException) : base(code, innerException)
        {
            Code = code;
            BookingId = bookingId;
            RequestingUserId = requestingUserId;

            Data["bookingId"] = bookingId;
            Data["requestingUserId"] = requestingUserId;
        }
    }
}
