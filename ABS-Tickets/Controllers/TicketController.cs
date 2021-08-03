using ABS_Tickets.Models;
using AirlineBookingSystem.Data.Interfaces;
using Common.ResponsesModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ABS_Tickets.Controllers
{
    [ApiController]
    [Route("ticket")]
    public class TicketController : ControllerBase
    {
        private ITicketDbService _ticketService;
        public TicketController(ITicketDbService service)
        {
            this._ticketService = service;
        }

        [HttpPost("create")]
        [Authorize]
        public IActionResult CreateTicket([FromBody] TicketCreateModel model)
        {

            try
            {
                var flightIds = model.FlightIds;
                var seats = model.Seats;
                var userId = GetUserIdFromTocken();

                for (int flightIndex = 0; flightIndex < seats.Length; flightIndex++)
                {
                    for (int seatIndex = 0; seatIndex < seats[flightIndex].Length; seatIndex++)
                    {
                        this._ticketService.CreateTicket(flightIds[flightIndex], seats[flightIndex][seatIndex].Id.ToString(), userId, seats[flightIndex][seatIndex].PassangerName);
                    }
                }

                return new OkObjectResult(new ResponseObject(true, "Seats booked successfully"));

            }
            catch (Exception e)
            {
                return BadRequest(new ResponseObject(false, "Something went wrong while creating tickets", e.Message));
            }

        }

        [HttpGet("user")]
        [Authorize]
        public IActionResult GetUserTickets()
        {
            try
            {
                var userId = GetUserIdFromTocken();
                var tickets = this._ticketService.GetUserTickets(userId);

                return new OkObjectResult(new ResponseObject(true, "User tickets here", tickets));
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseObject(false, "Could not get tickets, something went wrong", e.Message));
            }
        }


        private string GetUserIdFromTocken() => this.User.FindFirst("id")?.Value;
    }
}
