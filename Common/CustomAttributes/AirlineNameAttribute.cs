using ABS_Common.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Abs.Common.CustomAttributes
{
    public class AirlineNameAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                throw new ValidationException("Airline name is required");

            string name = value.ToString();

            if (name.Length > ModelConstants.AirlineNameMaxLegth || name.Length == 0 || !Regex.IsMatch(name, ModelConstants.NamePattern))
                throw new ValidationException("Airline name must consist of no more than 6 alphabetic upper cased characters");

            return ValidationResult.Success;

        }
    }
}
