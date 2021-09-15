using System.ComponentModel.DataAnnotations;

namespace Abs.Common.CustomAttributes
{
    public class CustomRequiredAttribute:ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                throw new ValidationException($"{validationContext.DisplayName} is required");
            }

            return ValidationResult.Success;
        }
    }
}
