using EventManagerService.Properties;
using System;
using System.ComponentModel.DataAnnotations;
using System.Resources;

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
#pragma warning disable CS8604 // Possible null reference argument.
                return new ValidationResult(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"),_otherPropertyName));
#pragma warning restore CS8604 // Possible null reference argument.
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
#pragma warning disable CS8604 // Possible null reference argument.
                    return new ValidationResult(ErrorMessage ?? string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"), validationContext.DisplayName, _otherPropertyName));
#pragma warning restore CS8604 // Possible null reference argument.
                }
            }

            return new ValidationResult(
#pragma warning disable CS8604 // Possible null reference argument.
                new ResourceManager(typeof(ErrorMessages)).GetString("NoComparableError")
#pragma warning restore CS8604 // Possible null reference argument.
            );
        }

    }
}
