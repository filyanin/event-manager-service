using System;

namespace EventManagerService.Domain.Exceptions
{
    // Простое доменное исключение валидации: хранит только код ошибки.
    // Структурированные данные доступны через свойства и Exception.Data.
    public class GreaterThenValidationException : DomainValidationException
    {
        public string Code { get; }

        public string FirstParamName { get; }

        public object? FirstParamValue { get; }

        public string SecondParamName { get; }

        public object? SecondParamValue { get; }

        /// <summary>
        /// Ожидалось, что FirstParamName будет больше, чем SecondParamName
        /// </summary>
        public GreaterThenValidationException(string code, string firstParamName, string secondParamName, object? firstParamValue, object? secondParamValue)
            : base(code)
        {
            Code = code;

            FirstParamName = firstParamName;
            SecondParamName = secondParamName;

            FirstParamValue = firstParamValue;
            SecondParamValue = secondParamValue;

            // Дополнительно кладём данные в Exception.Data для совместимости и удобства обработки
            Data["firstParamName"] = FirstParamName;
            Data["firstParamValue"] = FirstParamValue;
            Data["secondParamName"] = SecondParamName;
            Data["secondParamValue"] = SecondParamValue;
        }
        public GreaterThenValidationException(string code) : base(code)
        {
            Code = code;
        }

        public GreaterThenValidationException(string code, Exception inner) : base(code, inner)
        {
            Code = code;
        }

        /// <summary>
        /// Ожидалось, что FirstParamName будет больше, чем SecondParamName
        /// </summary>
        public GreaterThenValidationException(string code, string firstParamName, string secondParamName, object? firstParamValue, object? secondParamValue, Exception inner)
            : base(code, inner)
        {
            Code = code;

            FirstParamName = firstParamName;
            SecondParamName = secondParamName;

            FirstParamValue = firstParamValue;
            SecondParamValue = secondParamValue;

            Data["firstParamName"] = FirstParamName;
            Data["firstParamValue"] = FirstParamValue;
            Data["secondParamName"] = SecondParamName;
            Data["secondParamValue"] = SecondParamValue;
        }
    }
}
