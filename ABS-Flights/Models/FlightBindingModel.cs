using Abs.Common.CustomAttributes;
using ABS_Common.Common.CustomAttributes;
using System;

namespace ABS_Flights.Models
{
    public class FlightBindingModel : FlightBase
    {
        [CustomRequired]
        [NotPastDate]
        public DateTime DepartureDate { get; set; }

        [CustomRequired]
        [NotPastDate]
        public DateTime LandingDate { get; set; }

        [CustomRequired]
        public string FlightNumber { get; set; }

        [AirlineName]
        public string Airline { get; set; }

    }
}
