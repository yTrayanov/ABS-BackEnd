using ABS_Tickets.Models;
using ABS_Common.ResponsesModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using System.Linq;
using ABS_Data.Data;

namespace ABS_Tickets.Controllers
{
    [ApiController]
    [Route("ticket")]
    public class TicketController : ControllerBase
    {
        private IDbConnection _connection;

        public TicketController(ContextService contextService)
        {
            _connection = contextService.Connection;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateTicket([FromBody] TicketCreateModel model)
        {
            var flightIds = model.FlightIds;
            var seats = model.Seats;
            var username = GetUsernameFromTocken();

            for (int flightIndex = 0; flightIndex < seats.Length; flightIndex++)
            {
                for (int seatIndex = 0; seatIndex < seats[flightIndex].Length; seatIndex++)
                {

                    await _connection.QueryAsync($"EXEC usp_Tickets_Insert " +
                        $"'{username}'," +
                        $" {seats[flightIndex][seatIndex].Id}, " +
                        $"{flightIds[flightIndex]} ," +
                        $"{seats[flightIndex][seatIndex].PassengerName}");
                }
            }


            return new OkObjectResult(new ResponseObject("Seats booked successfully"));
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserTickets()
        {
            var username = GetUsernameFromTocken();

            using (var multi = await _connection.QueryMultipleAsync($"EXEC dbo.usp_UserTickets_Select {username}"))
            {
                var tickets = (await multi.ReadAsync<TicketViewModel>()).ToList();
                var flights = (await multi.ReadAsync<Flight>()).ToList();
                var seats = (await multi.ReadAsync<SeatModel>()).ToList();

                foreach (var ticket in tickets)
                {
                    ticket.Flight = flights.FirstOrDefault(f => f.Id == ticket.FlightId);
                    ticket.Seat = seats.FirstOrDefault(s => s.Id == ticket.SeatId);
                }

                return new OkObjectResult(new ResponseObject("User tickets here", tickets));
            }
        }
        private string GetUsernameFromTocken() => this.User.FindFirst("username")?.Value;
    }
}
