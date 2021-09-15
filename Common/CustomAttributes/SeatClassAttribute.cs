using ABS_Common.Enumerations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Abs.Common.CustomAttributes
{
    public class SeatClassAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            var firstLetter = value.ToString().Substring(0,1).ToUpper();

            string seatClass = value.ToString().Remove(0,1).Insert(0,firstLetter);


            if(!Enum.IsDefined(typeof(SeatClass) , seatClass))
            {
                throw new ValidationException("Invalid seatClass");
            }

            return ValidationResult.Success;
        }
    }
}
