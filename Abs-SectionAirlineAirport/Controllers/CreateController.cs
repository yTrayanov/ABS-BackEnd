using Abs_SectionAirlineAirport.Models;
using AirlineBookingSystem.Models;
using ABS_Common.ResponsesModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AirlineBookingSystem.Common;
using System.Data;
using Dapper;

namespace Abs_SectionAirlineAirport.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CreateController : ControllerBase
    {
        private IDbConnection _connection;

        public CreateController(ContextService contextService)
        {
            _connection = contextService.Connection;
        }
        [HttpPost("section")]
        public async Task<IActionResult> CreateSection([FromBody] SectionBindingModel sectionInfo)
        {
            var seatClass = sectionInfo.SeatClass.ToLower() == "first" ? SeatClass.First : sectionInfo.SeatClass.ToLower() == "bussiness" ? SeatClass.Bussiness : SeatClass.Economy;

            await _connection.QueryAsync<string>
                ($"EXEC usp_Sections_Insert {sectionInfo.Rows}, {sectionInfo.Columns}, {(int)seatClass}, '{sectionInfo.FlightNumber}' ");

            return new OkObjectResult(new ResponseObject( "Section created"));
        }
    }
}
