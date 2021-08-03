using AirlineBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABS_Auth.Common
{
    public interface IAuthService
    {
        IActionResult Login(string username, string password, string secret);

        IActionResult Register(string username , string password , string email);

        IActionResult Logout(string idFromToken);

        IActionResult CheckAuthorized(string idFromToken);

        IActionResult CheckCurrentUserStat(string id);
    }
}
