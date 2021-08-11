﻿using ABS_Auth.Helpers;
using AirlineBookingSystem.Models;
using ABS_Common.ResponsesModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ABS_Common;

namespace ABS_Auth.Common
{
    public class AuthService : IAuthService
    {
        private UserManager<User> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private SignInManager<User> _signInManager;

        public AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Logout(string idFromToken)
        {
            await _signInManager.SignOutAsync();
            return new OkObjectResult(new ResponseObject(true, "Logged out successfully"));
        }

        public async Task<IActionResult> Login(string username, string password, string secret)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
            if (result.Succeeded)
            {
                var user = this._signInManager.UserManager.Users.FirstOrDefault(u => u.UserName == username);

                string token = TokenService.GenerateJwtToken(user.Id, secret);
                bool isAdmin = await _userManager.IsInRoleAsync(user, Constants.AdminRole);

                LoginResponseModel data = new LoginResponseModel(token, isAdmin);
                return new OkObjectResult(new ResponseObject(true, "Logged in successfully", data));
            }

            return new BadRequestObjectResult(new ResponseObject(false, "Invalid username or password", "Invalid username or password"));

        }

        public async Task<IActionResult> CheckCurrentUserStat(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return new BadRequestObjectResult(new ResponseObject(false, "User with this id does not exist"));
            }

            bool isAdmin = await _userManager.IsInRoleAsync(user, Constants.AdminRole);
            var data = new LoginResponseModel(null, isAdmin);

            return new OkObjectResult(new ResponseObject(true, "User already logged", data));
        }

        public async Task<IActionResult> Register(string username, string password, string email)
        {
            var role = await _roleManager.FindByNameAsync(Constants.UserRole);
            User user = new User { UserName = username, Email = email };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return new BadRequestObjectResult(new ResponseObject(false, "Could not create user", result.Errors));

            result = await _userManager.AddToRoleAsync(user, role.Name);

            if (!result.Succeeded)
                return new BadRequestObjectResult(new ResponseObject(false, "Could not add role to user", result.Errors));

            return new OkObjectResult(new ResponseObject(true, "User registered"));
        }
    }
}
