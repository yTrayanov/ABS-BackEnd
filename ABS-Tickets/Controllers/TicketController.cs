using ABS_Tickets.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ABS_Tickets.Service;

namespace ABS_Tickets.Controllers
{
    [ApiController]
    [Route("ticket")]
    public class TicketController : ControllerBase
    {
        private ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateTicket([FromBody] TicketCreateModel model)
        {
            var username = GetUsernameFromTocken();

            return await _ticketService.CreateTicket(model, username);

        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserTickets()
        {
            var username = GetUsernameFromTocken();

            return await _ticketService.GetUserTickets(username);
        }
        private string GetUsernameFromTocken() => this.User.FindFirst("username")?.Value;
    }
}
