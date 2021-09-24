using ABS_Tickets.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABS_Tickets.Service
{
    public interface ITicketService
    {
        Task<IActionResult> CreateTicket(TicketCreateModel model , string username);
        Task<IActionResult> GetUserTickets(string username);
    }
}
