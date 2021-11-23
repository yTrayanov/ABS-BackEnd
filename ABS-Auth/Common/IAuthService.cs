using ABS_Auth.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABS_Auth.Common
{
    public interface IAuthService
    {
        Task<IActionResult> Login(UserModel user, string secret);

        Task<IActionResult> Register(UserModel userModel);

        Task<IActionResult> Logout(string username);

        Task<IActionResult> CheckCurrentUserStatAndRole(string username);

        Task<IActionResult> AuthorizeAdmin(string username);

        Task<IActionResult> Authorize(string username);
    }
}
