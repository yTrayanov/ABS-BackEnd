using ABS_Common.Common.CustomAttributes;
using System;

namespace ABS_Flights.Models
{
    public class TwoWaySearchModel : OneWayFlightModel
    {
        [NotPastDate("Cannot search for flight with past date")]
        public DateTime ReturnDate { get; set; }
    }
}
