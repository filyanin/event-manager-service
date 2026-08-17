namespace UserService.Application.Constants;

public static class ErrorCodes
{
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string UserAlreadyExistsError = "USER_ALREADY_EXISTS";
    public const string InvalidCredentialsError = "INVALID_CREDENTIALS";
    public const string UnauthorizedError = "UNAUTHORIZED";
    public const string NotFoundError = "NOT_FOUND";
    public const string ConflictError = "CONFLICT";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
}
