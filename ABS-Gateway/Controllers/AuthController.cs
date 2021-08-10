using ABS_Gateway.Common;
using ABS_Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Gateway.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        public AuthController(IOptions<APIUrls> urls, IClient client) : base(urls , client)
        {
            Client.BaseAddress = urls.Value.AuthApi;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] object body)
        {
            return await Client.Post(HttpContext, body);
        }

        [HttpGet("stat")]
        public async Task<IActionResult> GetCurrentUserStat()
        {
            return await Client.Get(HttpContext);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            return await Client.Get(HttpContext);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] object body)
        {
            return await Client.Post(HttpContext , body );
        }

    }
}
