using ABS_Common;
using ABS_Common.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Abs.Common.CustomAttributes
{
    public class AirportNameAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                throw new ValidationException("Airport name is required");

            string name = value.ToString();

            if (name.Length != ModelConstants.AirportNameLength || !Regex.IsMatch(name, ModelConstants.NamePattern))
                throw new ValidationException("Flight name must consist of 3 alphabetic upper cased characters");

            return ValidationResult.Success;
        }
    }
}
