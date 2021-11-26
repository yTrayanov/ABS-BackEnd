using ABS.Data.DynamoDbRepository;
using ABS_Common.ResponsesModels;
using ABS_Tickets.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABS_Tickets.Service
{
    public class TicketeService : ITicketService
    {
        private IRepository<string, TicketModel> _ticketRepository;

        public TicketeService( IRepository<string , TicketModel> ticketRepository)
        {
            this._ticketRepository = ticketRepository;
        }

        public async Task<IActionResult> CreateTicket(TicketCreateModel model, string username)
        {
            var flightIds = model.FlightIds;
            var seats = model.Seats;

            var tickets = new List<TicketModel>();

            for (int flightIndex = 0; flightIndex < flightIds.Length; flightIndex++)
            {
                for (int seatIndex = 0; seatIndex < seats[flightIndex].Length; seatIndex++)
                {
                    var currentSeat = seats[flightIndex][seatIndex];
                    tickets.Add(new TicketModel()
                    {
                        FlightId = flightIds[flightIndex],
                        SeatId = currentSeat.Id,
                        Username = username,
                        PassengerName = currentSeat.PassengerName,
                        SectionId = currentSeat.SectionId,
                    });
                }
            }

            await _ticketRepository.AddRange(tickets);

            return new OkObjectResult(new ResponseObject("Seats booked successfully"));
        }

        public async Task<IActionResult> GetUserTickets(string username)
        {
            var tickets = await _ticketRepository.GetList(username);
            return new OkObjectResult(new ResponseObject("User tickets here", tickets));
        }
    }

}
