using ABS_Common.ResponsesModels;
using ABS_Data.Data;
using ABS_Tickets.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABS_Tickets.Service
{
    public class TicketeService : ITicketService
    {
        private IDbConnection _connection;

        public TicketeService(ContextService contextService)
        {
            _connection = contextService.Connection;
        }
        public async Task<IActionResult> CreateTicket(TicketCreateModel model , string username)
        {
            var flightIds = model.FlightIds;
            var seats = model.Seats;

            var sb = new StringBuilder();

            for (int flightIndex = 0; flightIndex < seats.Length; flightIndex++)
            {
                for (int seatIndex = 0; seatIndex < seats[flightIndex].Length; seatIndex++)
                {
                    sb.AppendLine($"EXEC usp_Tickets_Insert " +
                        $"'{username}'," +
                        $" {seats[flightIndex][seatIndex].Id}, " +
                        $"{flightIds[flightIndex]} ," +
                        $"'{seats[flightIndex][seatIndex].PassengerName}';");
                }
            }

            await _connection.QueryAsync(sb.ToString());

            return new OkObjectResult(new ResponseObject("Seats booked successfully"));
        }

        public async Task<IActionResult> GetUserTickets(string username)
        {
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
    }
}
