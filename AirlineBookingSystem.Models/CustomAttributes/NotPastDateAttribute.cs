namespace AirlineBookingSystem.Models.CustomAttributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    public class NotPastDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {

            if(Convert.ToDateTime(value) < DateTime.Now)
            {
                throw new ValidationException("Can't create flights with past date");
            }

            return true;
        }
    }
}
