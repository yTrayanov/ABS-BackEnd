using Abs.Common.CustomAttributes;

namespace Abs_SectionAirlineAirport.Models
{
    public class AirportModel
    {
        [AirportName]
        public string Name { get; set; }
    }
}
