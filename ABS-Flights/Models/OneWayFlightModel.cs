using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Flights.Models
{
    public class OneWayFlightModel : FlightModel
    {
        public string DepartureDate { get; set; }
        public int MembersCount { get; set; }
    }
}
