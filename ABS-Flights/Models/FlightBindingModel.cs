using System;

namespace ABS_Flights.Models
{
    public class FlightBindingModel
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public int MembersCount { get; set; }
        public string DepartureDate { get; set; }

        public string LandingDate { get; set; }
        public string ReturnDate { get; set; }

        public string FlightNumber { get; set; }

        public string Airline { get; set; }

    }
}
