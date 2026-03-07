using EventManagerService.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Resources;
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
                return new ValidationResult(string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"),_otherPropertyName));
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
                    return new ValidationResult(ErrorMessage ?? string.Format(new ResourceManager(typeof(ErrorMessages)).GetString("GreaterThanValidationError"), validationContext.DisplayName, _otherPropertyName));
                }
            }

            return new ValidationResult(new ResourceManager(typeof(ErrorMessages)).GetString("NoComparableError"));
        }

    }
}
