using ABS_Common.Common.CustomAttributes;

namespace ABS_Flights.Models
{
    public class FlightBase
    {
        public string OriginAirport { get; set; }

        [NotEqual(nameof(OriginAirport))]
        public string DestinationAirport { get; set; }
    }
}
