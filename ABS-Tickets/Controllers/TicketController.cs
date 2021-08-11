using ABS_Tickets.Models;
using ABS_Common.ResponsesModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using AirlineBookingSystem.Data.Common;
using Microsoft.AspNetCore.Identity;
using AirlineBookingSystem.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ABS_Tickets.Controllers
{
    [ApiController]
    [Route("ticket")]
    public class TicketController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        private UserManager<User> _userManager;
        public TicketController(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            this._unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateTicket([FromBody] TicketCreateModel model)
        {
            var flightIds = model.FlightIds;
            var seats = model.Seats;
            var userId = GetUserIdFromTocken();

            var tickets = new List<Ticket>();

            for (int flightIndex = 0; flightIndex < seats.Length; flightIndex++)
            {
                for (int seatIndex = 0; seatIndex < seats[flightIndex].Length; seatIndex++)
                {
                    var flight = await _unitOfWork.Flights.Get(f => f.Id == flightIds[flightIndex]);
                    if (flight == null)
                        throw new ArgumentException("Flight does not exist");

                    var seat = await _unitOfWork.Seats.Get(s => s.Id == seats[flightIndex][seatIndex].Id);
                    if (seat == null)
                        throw new ArgumentException("Seat could not be found");

                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                        throw new ArgumentException("User could not be found");

                    var ticket = new Ticket()
                    {
                        UserId = user.Id,
                        SeatId = seat.Id,
                        FlightId = flight.Id,
                        PassengerName = seats[flightIndex][seatIndex].PassengerName
                    };

                    tickets.Add(ticket);

                    seat.IsBooked = true;
                    _unitOfWork.Seats.Update(seat);

                }
            }

            await _unitOfWork.Tickets.InsertRange(tickets);
            await _unitOfWork.Save();

            return new OkObjectResult(new ResponseObject(true, "Seats booked successfully"));
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserTickets()
        {
            var userId = GetUserIdFromTocken();

            var tickets = await _unitOfWork.Tickets.GetAll(t => t.UserId == userId,
                include: t => t.Include(t => t.Flight).ThenInclude(f => f.OriginAirport)
                             .Include(t => t.Flight).ThenInclude(f => f.DestinationAirport)
                             .Include(t => t.Flight).ThenInclude(f => f.Airline)
                             .Include(t => t.Seat).ThenInclude(s => s.Section)
                             .Include(t => t.User));

            return new OkObjectResult(new ResponseObject(true, "User tickets here", tickets));
        }


        private string GetUserIdFromTocken() => this.User.FindFirst("id")?.Value;
    }
}
