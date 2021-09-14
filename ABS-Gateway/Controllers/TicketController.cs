using ABS_Gateway.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ABS_Gateway.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [AuthorizeEndPoint]
    public class TicketController : BaseController
    {
        public TicketController(IOptions<APIUrls> urls, IClient client) : base(urls, client)
        {
            this.Client.BaseAddress = urls.Value.TicketApi;
        }

        [HttpPost("create")]
        [AuthorizeEndPoint]
        public async Task<IActionResult> CreateTicket([FromBody] object body)
        {
            return await Client.Post(HttpContext, body);
        }

        [HttpGet("user")]
        [AuthorizeEndPoint]
        public async Task<IActionResult> GetUserTickets()
        {
            return await Client.Get(HttpContext);
        }
    }
}
