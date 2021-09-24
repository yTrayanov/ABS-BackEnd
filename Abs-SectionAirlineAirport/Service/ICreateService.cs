using Abs_SectionAirlineAirport.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Abs_SectionAirlineAirport.Service
{
    public interface ICreateService
    {
        Task<IActionResult> CreateSection(SectionBindingModel sectionInfo);
        Task<IActionResult> CreateAirline(AirlineModel airlineInfo);
        Task<IActionResult> CreateAirport(AirportModel airportInfo);
    }
}
