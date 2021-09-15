using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abs.Common.CustomAttributes
{
    public class NumberInRange : ValidationAttribute
    {
        private int _maxValue;
        private int _minValue;

        public NumberInRange(int minValue , int maxValue)
        {
            this._maxValue = maxValue;
            this._minValue = minValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            int number;
            bool isNumber = int.TryParse(value.ToString(), out number);

            if (!isNumber)
            {
                throw new ValidationException($"{validationContext.DisplayName} should be a number");
            }
            else if(number < _minValue || number > _maxValue )
            {
                throw new ValidationException($"{validationContext.DisplayName} value must be a number between {_minValue} and {_maxValue}");
            }

            return ValidationResult.Success;
        }
    }
}
