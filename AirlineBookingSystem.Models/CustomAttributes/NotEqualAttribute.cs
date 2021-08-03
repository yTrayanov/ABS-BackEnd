namespace AirlineBookingSystem.Models.CustomAttributes
{
    using System.ComponentModel.DataAnnotations;

    public class NotEqualAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "{0} cannot be the same as {1}.";
        public NotEqualAttribute(string otherProperty) : base(DefaultErrorMessage)
        {
            this.OtherProperty = otherProperty;
        }

        public string OtherProperty { get; private set; }

        
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value != null)
            {
                var otherProperty = validationContext.ObjectInstance.GetType().GetProperty(this.OtherProperty);

                var otherPropertyValue = otherProperty
                    .GetValue(validationContext.ObjectInstance, null);

                if (value.Equals(otherPropertyValue))
                {
                    throw new ValidationException(FormatErrorMessage(validationContext.DisplayName));
                }
            }

            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string currentPropertyName)
        {
            return string.Format(this.ErrorMessageString, currentPropertyName, this.OtherProperty);
        }

    }
}
