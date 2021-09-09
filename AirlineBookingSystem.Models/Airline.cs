namespace AirlineBookingSystem.Models
{
    using AirlineBookingSystem.Models.Constants;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Airline:BaseModel
    {
        [Required(ErrorMessage ="Airline name is required")]
        [MaxLength(ModelConstants.AirlineNameMaxLegth , ErrorMessage ="Airline must be less then 6 alphabetic symbols")]
        [RegularExpression(ModelConstants.NamePattern , ErrorMessage ="Invalid Airline format")]
        public string Name{ get; set; }


    }
}
