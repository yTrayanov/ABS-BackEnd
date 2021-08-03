using ABS_Auth.Helpers;
using AirlineBookingSystem.Data.Interfaces;
using AirlineBookingSystem.Models;
using Common;
using Common.ResponsesModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ABS_Auth.Common
{
    public class AuthService : IAuthService
    {
        private IUserDbService _userService;
        private IRoleDbService _roleService;
        public AuthService(IUserDbService userService , IRoleDbService roleService)
        {
            this._userService = userService;
            this._roleService = roleService;
        }

        public IActionResult CheckAuthorized(string idFromToken)
        {
            try
            {
                User user = this._userService.GetUser(idFromToken);

                if (user.Status != UserStatus.LoggedIn)
                    return new UnauthorizedObjectResult(new ResponseObject(false, "User is not authorized"));


                return new OkObjectResult(new ResponseObject(true, "User is authorized"));
            }
            catch (Exception e)
            {
                return new UnauthorizedObjectResult(new ResponseObject(false, "User is not authorized" , e.Message));

            }

        }

        public IActionResult Logout(string idFromToken)
        {
            try
            {
                this._userService.LogoutUser(idFromToken);
                return new OkObjectResult(new ResponseObject(true, "Logged out successfully"));
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new ResponseObject(false, "Could not login", e.Message));
            }
        }

        public IActionResult Login(string username, string password, string secret)
        {
            try
            {
                User user = this._userService.LoginUser(username, password);

                string token = TokenService.GenerateJwtToken(user.Id, secret);
                var userRoles = this._roleService.GetUserRoles(user);
                bool isAdmin = userRoles.Any(r => r.Name == "Admin");

                LoginResponseModel data = new LoginResponseModel(token, isAdmin);
                return new OkObjectResult(new ResponseObject(true, "Logged in successfully", data));
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new ResponseObject(false, "Could not login", e.Message));
            }

        }

        public IActionResult CheckCurrentUserStat(string id)
        {
            try
            {
                var user = this._userService.GetUser(id);
                user.Status = UserStatus.LoggedIn;

                var userRoles = this._roleService.GetUserRoles(user);
                bool isAdmin = userRoles.Any(r => r.Name == "Admin");

                var data = new LoginResponseModel(null, isAdmin);

                return new OkObjectResult(new ResponseObject(true, "User is already logged", data));

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new ResponseObject(false, e.Message, e.Message));
            }

        }

        public IActionResult Register(string username, string password, string email)
        {

            try
            {
                var user = new User() { Username = username, HashedPassword = password, Email = email };
                this._userService.RegisterUser(user);
                this._roleService.AddRoleToUser(user, "User");

                return new OkObjectResult(new ResponseObject(true, "User registered successfully"));
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new ResponseObject(false, e.Message, e.Message));
            }

            throw new NotImplementedException();
        }
    }
}
