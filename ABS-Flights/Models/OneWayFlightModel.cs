using ABS_Common.Common.CustomAttributes;
using System;

namespace ABS_Flights.Models
{
    public class OneWayFlightModel : FlightBase
    {
        [NotPastDate]
        public DateTime DepartureDate { get; set; }
        public int MembersCount { get; set; }
    }
}
