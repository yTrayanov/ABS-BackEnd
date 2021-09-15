using Abs.Common.CustomAttributes;
using ABS_Common.Common.CustomAttributes;
using System;

namespace ABS_Tickets.Models
{
    public class Flight
    {
        public int Id { get; set; }

        [CustomRequired]
        public string FlightNumber { get; set; }

        [AirlineName]
        public string Airline { get; set; }

        [AirportName]
        public string OriginAirport { get; set; }

        [AirportName]
        public string DestinationAirport { get; set; }

        [NotPastDate]
        public DateTime DepartureDate { get; set; }

        [NotPastDate]
        public DateTime LandingDate {get;set; }
    }
}
