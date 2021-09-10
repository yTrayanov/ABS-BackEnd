using System;

namespace ABS_Flights.Models
{
    public class FlightBindingModel : FlightBase
    {
        public DateTime DepartureDate { get; set; }

        public DateTime LandingDate { get; set; }

        public string FlightNumber { get; set; }

        public string Airline { get; set; }

    }
}
