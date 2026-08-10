namespace EventManagerService.Shared.ErrorCodes;

public static class ErrorCodes
{
    // Строковые стабильные коды ошибок, использовать в API/логике как единую точку правды
    public const string Unknown = "UNKNOWN_ERROR";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string HighLoad = "HIGH_LOAD";
    public const string GreaterThanValidationError = "GreaterThanValidationError";
    public const string TryChangeCompletedBookingError = "TryChangeCompletedBookingError";
    public const string NoAvailableSeatsError = "NoAvailableSeatsError";
    public const string BookingNotFoundWhenProcessing = "BookingNotFoundWhenProcessing";
    public const string LengthValidationError = "LengthValidationError";
    public const string NoEnoughAvailableSeatsError = "NoEnoughAvailableSeatsError";
    public const string WrongReleaseSeatsCountError = "WrongReleaseSeatsCountError";
    public const string NoAviableSeatsError = "NoAviableSeatsError"; // legacy typo used in domain, keep for compatibility
    public const string PastEventBookingError = "PAST_EVENT_BOOKING_ERROR";
    public const string ActiveBookingsLimitExceededError = "ACTIVE_BOOKINGS_LIMIT_EXCEEDED_ERROR";
    public const string UnauthorizedBookingCancellationError = "UNAUTHORIZED_BOOKING_CANCELLATION_ERROR";
    public const string UnauthorizedOperationError = "UNAUTHORIZED_OPERATION_ERROR";
    public const string InvalidCredentialsError = "INVALID_CREDENTIALS_ERROR";
    public const string UserAlreadyExistsError = "USER_ALREADY_EXISTS_ERROR";
    // Добавляйте сюда новые коды по мере необходимости
}
