using ABS_Auth.Common;
using ABS_Auth.Helpers;
using ABS_Auth.Models;
using Common;
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
        public IActionResult Stat()
        {
            string userId = GetUserIdFromTocken();

            return this._authService.CheckCurrentUserStat(userId);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel userInfo)
        {
            return _authService.Login(userInfo.Username, userInfo.Password, _secret);
        }

        [HttpGet("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            string userid = GetUserIdFromTocken();
            return _authService.Logout(userid);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel userInfo)
        {
            return this._authService.Register(userInfo.Username , userInfo.Password , userInfo.Email);
        }
        private string GetUserIdFromTocken() => this.User.FindFirst("id")?.Value;

    }
}
