using ABS_Common.Enumerations;
using ABS_Common.ResponsesModels;
using ABS_Data.Data;
using Abs_SectionAirlineAirport.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Threading.Tasks;

namespace Abs_SectionAirlineAirport.Service
{
    public class CreateService : ICreateService
    {


        private IDbConnection _connection;

        public CreateService(ContextService contextService)
        {
            _connection = contextService.Connection;
        }

        public async Task<IActionResult> CreateAirline(AirlineModel airlineInfo)
        {
            string query = $"EXEC usp_Airline_Insert @Name";

            await _connection.QueryAsync(query, new { Name = airlineInfo.Name });

            return new OkObjectResult(new ResponseObject("Airline created successfully"));
        }

        public async Task<IActionResult> CreateAirport(AirportModel airportInfo)
        {
            string query = $"EXEC usp_Airport_Insert @Name";

            await _connection.QueryAsync(query, new { Name = airportInfo.Name });

            return new OkObjectResult(new ResponseObject("Airport created successfully"));
        }

        public async Task<IActionResult> CreateSection(SectionBindingModel sectionInfo)
        {
            var seatClass = sectionInfo.SeatClass.ToLower() == "first" ? SeatClass.First : sectionInfo.SeatClass.ToLower() == "bussiness" ? SeatClass.Bussiness : SeatClass.Economy;

            string query = $"EXEC usp_Sections_Insert @Rows, @Columns, @SeatClass, @FlightNumber";
            var parameters = new { Rows = sectionInfo.Rows, Columns = sectionInfo.Columns, SeatClass = (int)seatClass, FlightNumber = sectionInfo.FlightNumber };

            await _connection.QueryAsync<string>(query, parameters);

            return new OkObjectResult(new ResponseObject("Section created"));
        }
    }
}
