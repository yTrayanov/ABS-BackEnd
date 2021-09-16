namespace ABS_Common.Common.CustomAttributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    public class NotPastDateAttribute : ValidationAttribute
    {
        public NotPastDateAttribute(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }
        public override bool IsValid(object value)
        {

            if(Convert.ToDateTime(value) < DateTime.Now)
            {
                throw new ValidationException(ErrorMessage);
            }

            return true;
        }
    }
}
