using Abs.Common.CustomAttributes;

namespace Abs_SectionAirlineAirport.Models
{
    public class SectionBindingModel
    {
        [NumberInRange(1,100)]
        public int Rows { get; set; }

        [NumberInRange(1,10)]
        public int Columns { get; set; }

        [CustomRequired]
        [SeatClass]
        public string SeatClass { get; set; }

        [CustomRequired]
        public string FlightNumber { get; set; }
    }
}
