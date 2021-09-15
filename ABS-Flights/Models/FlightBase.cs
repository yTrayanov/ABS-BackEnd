using Abs.Common.CustomAttributes;
using ABS_Common.Common.CustomAttributes;

namespace ABS_Flights.Models
{
    public class FlightBase
    {
        
        [AirportName]
        public string OriginAirport { get; set; }

        [AirportName]
        [NotEqual(nameof(OriginAirport))]
        public string DestinationAirport { get; set; }
    }
}
