using System;

using EventManagerService.Shared.Exceptions;

namespace EventManagerService.Domain.Exceptions
{
    /// <summary>
    /// Базовое доменное исключение валидации. Нужен для групповой обработки всех доменных ошибок валидации.
    /// Наследуется от AppException, чтобы переносить централизованный ErrorCode.
    /// </summary>
    public class DomainValidationException : AppException
    {
        public DomainValidationException()
            : base("DOMAIN_VALIDATION_ERROR")
        {
        }

        public DomainValidationException(string code)
            : base(code)
        {
        }

        public DomainValidationException(string code, Exception inner)
            : base(code, inner)
        {
        }
    }
}
