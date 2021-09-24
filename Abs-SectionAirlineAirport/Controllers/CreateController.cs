using Abs_SectionAirlineAirport.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Abs_SectionAirlineAirport.Service;

namespace Abs_SectionAirlineAirport.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CreateController : ControllerBase
    {
        private ICreateService _createService;

        public CreateController(ICreateService createService)
        {
            _createService = createService;
        }
        [HttpPost("section")]
        public async Task<IActionResult> CreateSection([FromBody] SectionBindingModel sectionInfo)
        {
            return await _createService.CreateSection(sectionInfo);
        }

        [HttpPost("airline")]
        public async Task<IActionResult> CreateAirline([FromBody] AirlineModel airlineInfo)
        {
            return await _createService.CreateAirline(airlineInfo);
        }

        [HttpPost("airport")]
        public async Task<IActionResult> CreateAirport([FromBody] AirportModel airportInfo)
        {
            return await _createService.CreateAirport(airportInfo);
        }
    }
}
