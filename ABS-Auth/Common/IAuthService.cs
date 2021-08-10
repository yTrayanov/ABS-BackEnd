using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABS_Auth.Common
{
    public interface IAuthService
    {
        Task<IActionResult> Login(string username, string password, string secret);

        Task<IActionResult> Register(string username , string password , string email);

        Task<IActionResult> Logout(string idFromToken);

        Task<IActionResult> CheckCurrentUserStat(string id);
    }
}
