using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABS_Auth.Common
{
    public interface IAuthService
    {
        Task<IActionResult> Login(string username, string password, string secret);

        Task<IActionResult> Register(string username , string password , string email);

        Task<IActionResult> Logout(string username);

        Task<IActionResult> CheckCurrentUserStatAndRole(string username);

        Task<IActionResult> AuthorizeAdmin(string username);

        Task<IActionResult> Authorize(string username);
    }
}
