using System.ComponentModel.DataAnnotations;

namespace EventManagerService.Application.Validators
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
                return new ValidationResult("");
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
                    return new ValidationResult(ErrorMessage ?? "");

                }
            }

            return new ValidationResult(
                ""
            );
        }

    }
}
