using System;

namespace ABS_Flights.Models
{
    public class FlightBindingModel : FlightBase
    {
        public int MembersCount { get; set; }
        public DateTime DepartureDate { get; set; }

        public DateTime LandingDate { get; set; }
        public string ReturnDate { get; set; }

        public string FlightNumber { get; set; }

        public string Airline { get; set; }

    }
}
