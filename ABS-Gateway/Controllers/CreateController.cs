﻿using ABS_Gateway.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ABS_Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AuthorizeAdminEndPoint]
    public class CreateController : BaseController
    {
        public CreateController(IOptions<APIUrls> urls, IClient client) : base(urls, client)
        {
            this.Client.BaseAddress = urls.Value.CreateApi;
        }


        [HttpPost("section")]
        [AuthorizeAdminEndPoint]
        public async Task<IActionResult> CreateSection([FromBody] object body)
        {
            return await Client.Post(HttpContext, body);
        }

        [HttpPost("Airline")]
        [AuthorizeAdminEndPoint]
        public async Task<IActionResult> CreateAirline([FromBody] object body)
        {
            return await Client.Post(HttpContext, body);
        }

        [HttpPost("Airport")]
        [AuthorizeAdminEndPoint]
        public async Task<IActionResult> CreateAirport([FromBody] object body)
        {
            return await Client.Post(HttpContext, body);
        }

    }
}
