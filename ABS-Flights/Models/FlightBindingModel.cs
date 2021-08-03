using System;

namespace ABS_Flights.Models
{
    public class FlightBindingModel
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public DateTime DepartureDate { get; set; }
        public int MembersCount { get; set; }

        public DateTime ReturnDate { get; set; }

        public string FlightNumber { get; set; }

        public string Airline { get; set; }

        public DateTime LandingDate { get; set; }
    }
}
