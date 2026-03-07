using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventManagerService.Presentation.Validators
{
    public class GreaterThanAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;

        public GreaterThanAttribute(string otherPropertyName)
        {
            _otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var otherPropertyInfo = validationContext.ObjectType.GetProperty(_otherPropertyName);

            if (otherPropertyInfo == null)
            {
                return new ValidationResult($"Unknown property: {_otherPropertyName}");
            }

            var otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);

            if (value is IComparable comparableValue && otherPropertyValue is IComparable comparableOther)
            {
                if (comparableValue.CompareTo(comparableOther) > 0)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be greater than {_otherPropertyName}.");
                }
            }

            return new ValidationResult("This attribute can only be used on comparable types.");
        }

    }
}
