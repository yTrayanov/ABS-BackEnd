using AirlineBookingSystem.Models.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirlineBookingSystem.Models
{
    public class Flight:BaseModel
    {

        [Required(ErrorMessage = "Flight id is required")]
        public string FlightNumber { get; set; }
        public string Airline { get; set; }

        public string OriginAirport { get; set; }

        [NotEqual(nameof(OriginAirport))]
        public string DestinationAirport { get; set; }

        public ICollection<Section> Sections { get; set; }


        [NotPastDate]
        public DateTime DepartureDate { get; set; }


        [NotPastDate]
        public DateTime LandingDate {get;set; }

        public ICollection<Ticket> Tickets { get; set; }
    }
}
