using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Flights.Models
{
    public class TwoWaySearchModel : OneWayFlightModel
    {
        public string ReturnDate { get; set; }
    }
}
