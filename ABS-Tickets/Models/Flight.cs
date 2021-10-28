using Abs.Common.CustomAttributes;
using ABS_Common.Common.CustomAttributes;
using System;

namespace ABS_Tickets.Models
{
    public class Flight
    {
        public string Id { get; set; }

        [CustomRequired]
        public string FlightNumber { get; set; }

        [AirlineName]
        public string Airline { get; set; }

        [AirportName]
        public string OriginAirport { get; set; }

        [AirportName]
        public string DestinationAirport { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime LandingDate {get;set; }
    }
}
