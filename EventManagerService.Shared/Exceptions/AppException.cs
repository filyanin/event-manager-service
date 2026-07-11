using System;

namespace EventManagerService.Shared.Exceptions
{
    // Базовое приложение-ориентированное исключение, содержит централизованный ErrorCode
    public class AppException : Exception
    {
        public string ErrorCode { get; }

        public AppException(string errorCode)
            : base(errorCode)
        {
            ErrorCode = errorCode;
        }

        public AppException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public AppException(string errorCode, Exception inner)
            : base(errorCode, inner)
        {
            ErrorCode = errorCode;
        }
    }
}
