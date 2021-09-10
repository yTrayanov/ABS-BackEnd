using System.Collections.Generic;

namespace ABS_Flights.Models
{
    public class FlightWithSectionsModel : FlightBase
    {
        public int Id { get; set; }
        public ICollection<Section> Sections { get; set; }
    }
}
