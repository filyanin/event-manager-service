using System;

namespace EventManagerService.Domain.Exceptions
{
    /// <summary>
    /// Базовое доменное исключение валидации. Нужен для групповой обработки всех доменных ошибок валидации.
    /// </summary>
    public class DomainValidationException : Exception
    {
        public DomainValidationException()
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
