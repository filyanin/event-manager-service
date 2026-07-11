using System;

namespace EventManagerService.Domain.Exceptions
{
    // Исключение для проверки диапазона (min / max)
    public class RangeValidationException : DomainValidationException
    {
        public string Code { get; }

        public string TargetName { get; }
        public object? TargetValue { get; }

        public string MinName { get; }
        public object? MinValue { get; }

        public string MaxName { get; }
        public object? MaxValue { get; }

        public RangeValidationException(string code, string targetName, string minName, string maxName, object? targetValue, object? minValue, object? maxValue)
            : base(code)
        {
            Code = code;

            TargetName = targetName;
            TargetValue = targetValue;

            MinName = minName;
            MinValue = minValue;

            MaxName = maxName;
            MaxValue = maxValue;

            Data["targetName"] = TargetName;
            Data["targetValue"] = TargetValue;
            Data["minName"] = MinName;
            Data["minValue"] = MinValue;
            Data["maxName"] = MaxName;
            Data["maxValue"] = MaxValue;
        }

        public RangeValidationException(string code, string targetName, string minName, string maxName, object? targetValue, object? minValue, object? maxValue, Exception inner)
            : base(code, inner)
        {
            Code = code;

            TargetName = targetName;
            TargetValue = targetValue;

            MinName = minName;
            MinValue = minValue;

            MaxName = maxName;
            MaxValue = maxValue;

            Data["targetName"] = TargetName;
            Data["targetValue"] = TargetValue;
            Data["minName"] = MinName;
            Data["minValue"] = MinValue;
            Data["maxName"] = MaxName;
            Data["maxValue"] = MaxValue;
        }
    }
}
