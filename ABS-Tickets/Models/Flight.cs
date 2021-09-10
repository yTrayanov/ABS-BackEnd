using System;

namespace ABS_Tickets.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; }
        public string Airline { get; set; }

        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime LandingDate {get;set; }
    }
}
