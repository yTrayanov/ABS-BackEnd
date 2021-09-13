using ABS_Auth.Common;
using ABS_Auth.Models;
using ABS_Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ABS_Auth.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly string _secret;

        public AuthController(IOptions<AppSettings> appSettings, IAuthService authService)
        {
            this._secret = appSettings.Value.Secret;
            this._authService = authService;
        }

        [HttpGet("stat")]
        [Authorize]
        public async Task<IActionResult> Stat()
        {
            string username = GetUsernameFromToken();
            return await this._authService.CheckCurrentUserStatAndRole(username);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel userInfo)
        {
            return await _authService.Login(userInfo.Username, userInfo.Password, _secret);
        }

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            string username = GetUsernameFromToken();
            return await _authService.Logout(username);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel userInfo)
        {
            return await this._authService.Register(userInfo.Username, userInfo.Password, userInfo.Email);
        }

        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize()
        {
            var username = GetUsernameFromToken();

            return await this._authService.Authorize(username);
        }

        [HttpGet("authorize/admin")]
        [Authorize]
        public async Task<IActionResult> AuthorizeAdmin()
        {
            string username = GetUsernameFromToken();
            return await _authService.AuthorizeAdmin(username);
        }


        private string GetUsernameFromToken() => this.User.FindFirst("username")?.Value;

    }
}
