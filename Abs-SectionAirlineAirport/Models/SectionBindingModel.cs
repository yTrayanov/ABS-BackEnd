using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abs_SectionAirlineAirport.Models
{
    public class SectionBindingModel
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string SeatClass { get; set; }
        public string FlightNumber { get; set; }
    }
}
