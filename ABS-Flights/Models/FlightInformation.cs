using Abs.Common.CustomAttributes;

namespace ABS_Flights.Models
{
    public class FlightInformation : FlightWithSectionsModel
    {
        [AirlineName]
        public string Airline { get; set; }

        [CustomRequired]
        public string FlightNumber { get; set; }
    }
}
