namespace UserService.Application.Exceptions;

/// <summary>
/// Базовое исключение приложения
/// </summary>
public class AppException : Exception
{
    public string ErrorCode { get; set; }

    public AppException(string errorCode, string? message = null) 
        : base(message ?? errorCode)
    {
        ErrorCode = errorCode;
    }

    public AppException(string errorCode, Exception innerException) 
        : base(errorCode, innerException)
    {
        ErrorCode = errorCode;
    }
}
