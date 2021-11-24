using ABS_Common.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abs_SectionAirlineAirport.Models
{
    public class SectionModel
    {
        public string Id { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }

        public string FlightNumber { get; set; }

        public SeatClass SeatClass { get; set; }
        public int AvailableSeats { get; set; }


    }
}
