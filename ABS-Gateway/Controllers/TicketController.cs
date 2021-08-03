using ABS_Gateway.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Gateway.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class TicketController : BaseController
    {
        public TicketController(IOptions<APIUrls> urls, IClient client) : base(urls, client)
        {
            this.Client.BaseAddress = urls.Value.TicketApi;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTicket([FromBody] object body)
        {
            return await Client.Post(HttpContext, body);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserTickets()
        {
            return await Client.Get(HttpContext);
        }
    }
}
