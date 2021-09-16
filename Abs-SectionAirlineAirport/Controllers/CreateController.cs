using Abs_SectionAirlineAirport.Models;
using ABS_Common.ResponsesModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using ABS_Data.Data;
using ABS_Common.Enumerations;

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

            string query = $"EXEC usp_Sections_Insert @Rows, @Columns, @SeatClass, @FlightNumber";
            var parameters = new { Rows = sectionInfo.Rows, Columns = sectionInfo.Columns, SeatClass = (int)seatClass, FlightNumber = sectionInfo.FlightNumber };

            await _connection.QueryAsync<string>(query, parameters);

            return new OkObjectResult(new ResponseObject( "Section created"));
        }

        [HttpPost("airline")]
        public async Task<IActionResult> CreateAirline([FromBody] GeneralModel data)
        {
            string query = $"EXEC usp_Airline_Insert @Name";

            await _connection.QueryAsync(query, new { Name = data.Name});

            return new OkObjectResult(new ResponseObject("Airline created successfully"));
        }

        [HttpPost("airport")]
        public async Task<IActionResult> CreateAirport([FromBody] GeneralModel data)
        {
            string query = $"EXEC usp_Airport_Insert @Name";

            await _connection.QueryAsync(query, new { Name = data.Name });

            return new OkObjectResult(new ResponseObject("Airport created successfully"));
        }
    }
}
