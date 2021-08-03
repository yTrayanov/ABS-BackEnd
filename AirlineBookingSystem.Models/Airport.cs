namespace AirlineBookingSystem.Models
{
    using AirlineBookingSystem.Models.Constants;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Airport:BaseModel
    {

        public Airport()
        {
            this.IncomingFlights = new List<Flight>();
            this.DepartingFlights = new List<Flight>();
        }

        [Required(ErrorMessage = "Airport name is required")]
        [StringLength(ModelConstants.AirportNameLength, MinimumLength = ModelConstants.AirportNameLength, ErrorMessage = "Flight name must consist of 3 alphabetic upper cased characters")]
        [RegularExpression(ModelConstants.NamePattern, ErrorMessage = "Name should contain only uppercased letters!")]
        public string Name { get; set; }

        public ICollection<Flight> IncomingFlights { get; set; }
        public ICollection<Flight> DepartingFlights { get; set; }

    }
}
