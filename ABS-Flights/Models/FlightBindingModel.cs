using ABS_Common.Common.CustomAttributes;
using System;

namespace ABS_Flights.Models
{
    public class FlightBindingModel : FlightBase
    {
        [NotPastDate]
        public DateTime DepartureDate { get; set; }

        [NotPastDate]
        public DateTime LandingDate { get; set; }

        public string FlightNumber { get; set; }

        public string Airline { get; set; }

    }
}
