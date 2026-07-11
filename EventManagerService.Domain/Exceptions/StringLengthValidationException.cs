using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Domain.Exceptions
{
    public class StringLengthValidationException : DomainValidationException
    {
        public string Code { get; }

        public string FirstParamName { get; }

        public object? FirstParamValue { get; }

        public string SecondParamName { get; }

        public object? SecondParamValue { get; }

        /// <summary>
        /// Ожидалось, что длина строки будет больше FirstParamName и меньше, чем SecondParamName
        /// </summary>
        public StringLengthValidationException(string code, string firstParamName, string secondParamName, object? firstParamValue, object? secondParamValue)
            : base(code)
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

        /// <summary>
        /// Ожидалось, что длина строки будет больше FirstParamName и меньше, чем SecondParamName
        /// </summary>
        public StringLengthValidationException(string code, string firstParamName, string secondParamName, object? firstParamValue, object? secondParamValue, Exception inner)
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
