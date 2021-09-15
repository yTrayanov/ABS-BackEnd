using ABS_Common.Common.CustomAttributes;
using System;

namespace ABS_Flights.Models
{
    public class TwoWaySearchModel : OneWayFlightModel
    {
        [NotPastDate]
        public DateTime ReturnDate { get; set; }
    }
}
