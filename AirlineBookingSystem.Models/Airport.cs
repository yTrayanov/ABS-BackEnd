namespace AirlineBookingSystem.Models
{
    using AirlineBookingSystem.Models.Constants;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Airport:BaseModel
    {

        [Required(ErrorMessage = "Airport name is required")]
        [StringLength(ModelConstants.AirportNameLength, MinimumLength = ModelConstants.AirportNameLength, ErrorMessage = "Flight name must consist of 3 alphabetic upper cased characters")]
        [RegularExpression(ModelConstants.NamePattern, ErrorMessage = "Name should contain only uppercased letters!")]
        public string Name { get; set; }

    }
}
