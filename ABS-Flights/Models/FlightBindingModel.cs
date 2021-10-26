using Abs.Common.CustomAttributes;
using ABS_Common.Common.CustomAttributes;
using System;
using System.Collections.Generic;

namespace ABS_Flights.Models
{
    public class FlightBindingModel : FlightBase
    {
        public string Id { get; set; }

        [CustomRequired]
        [NotPastDate("Cannot create flights with past date")]
        public DateTime DepartureDate { get; set; }

        [CustomRequired]
        [NotPastDate("Can't create flights with past date")]
        public DateTime LandingDate { get; set; }

        [CustomRequired]
        public string FlightNumber { get; set; }

        [AirlineName]
        public string Airline { get; set; }

        public ICollection<Section> Sections { get; set; }
    }
}
