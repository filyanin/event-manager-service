using System;

namespace EventManagerService.Domain.Exceptions
{
    // Простое доменное исключение валидации: хранит только код ошибки.
    // Все структурированные данные должны передаваться через Exception.Data.
    public class DomainValidationException : Exception
    {
        public string Code { get; }

        public DomainValidationException(string code)
            : base(code)
        {
            Code = code;
        }

        public DomainValidationException(string code, Exception inner)
            : base(code, inner)
        {
            Code = code;
        }
    }
}
