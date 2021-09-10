using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Data;
using Dapper;
using ABS_Auth.Helpers;
using ABS_Common;
using System.Linq;
using ABS_Common.ResponsesModels;
using ABS_Data.Data;

namespace ABS_Auth.Common
{
    public class AuthService : IAuthService
    {
        private readonly IDbConnection _connection;
        public AuthService(ContextService contextService)
        {
            _connection = contextService.Connection;
        }

        public async Task<IActionResult> Logout(string username)
        {
            await _connection.QueryAsync($"EXEC usp_LogoutUser_Update '{username}',{(int)UserStatus.Registered} ");
            return new OkObjectResult(new ResponseObject("Logged out successfully"));
        }

        public async Task<IActionResult> Login(string username, string password, string secret)
        {
            var userRoles = await _connection.QueryAsync<string>($"EXEC ups_LoginUser_Update '{username}','{password}',{(int)UserStatus.LoggedIn} ");
            string token = TokenService.GenerateJwtToken(username, secret);
            bool isAdmin = userRoles.Contains(Constants.AdminRole);

            LoginResponseModel data = new LoginResponseModel(token, isAdmin);
            return new OkObjectResult(new ResponseObject("Logged in successfully", data));

        }

        public async Task<IActionResult> CheckCurrentUserStat(string username)
        {
            var userRoles = await _connection.QueryAsync<string>($"EXEC ups_GetUserRoles_Select '{username}', {(int)UserStatus.LoggedIn}");
            bool isAdmin = userRoles.Contains(Constants.AdminRole);
            var data = new LoginResponseModel(null, isAdmin);
            return new OkObjectResult(new ResponseObject("User already logged", data));
        }

        public async Task<IActionResult> Register(string username, string password, string email)
        {
            await _connection.QueryAsync($"EXEC usp_RegisterUsers_Insert '{username}', '{password}', '{email}'");

            return new OkObjectResult(new ResponseObject("User registered"));
        }

        public async Task<IActionResult> AuthorizeAdmin(string username)
        {
            var userRoles = await _connection.QueryAsync<string>($"EXEC ups_GetUserRoles_Select '{username}', {(int)UserStatus.LoggedIn}");

            if (userRoles.Contains(Constants.AdminRole))
                return new OkResult();

            return new UnauthorizedResult();
        }

    }
}
