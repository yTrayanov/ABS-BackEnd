using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using ABS_Auth.Helpers;
using ABS_Common.Constants;
using ABS_Common.ResponsesModels;
using Amazon.DynamoDBv2.Model;
using Abs.Common.Constants;
using ABS_Auth.Models;
using ABS.Data.DynamoDbRepository;

namespace ABS_Auth.Common
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<string, UserModel> _userRepository;
        public AuthService(IRepository<string , UserModel> repository )
        {
            _userRepository = repository;
        }

        public async Task<IActionResult> Logout(string username)
        {
            var userModel = new UserModel
            {
                Username = username,
                Status = UserStatus.Registered
            };

            await _userRepository.Update(userModel);

            return new OkObjectResult(new ResponseObject("Logged out successfully"));
        }

        public async Task<IActionResult> Login(UserModel user, string secret)
        {
            
            try
            {
                var responseItem = await _userRepository.Update(user);

                string token = TokenService.GenerateJwtToken(user.Username, secret);

                bool isAdmin = responseItem.Role == Constants.AdminRole;
                LoginResponseModel data = new LoginResponseModel(token, isAdmin);
                return new OkObjectResult(new ResponseObject("Logged in successfully", data));
            }
            catch (ConditionalCheckFailedException)
            {
                throw new ArgumentException(ErrorMessages.InvalidCredentials);
            }
        }

        public async Task<IActionResult> CheckCurrentUserStatAndRole(string username)
        {
            var user = await GetUser(username);
            if (user == null)
                throw new ArgumentException(ErrorMessages.UserNotFound);

            bool isAdmin = user.Role == Constants.AdminRole;
            var data = new LoginResponseModel(null, isAdmin);
            return new OkObjectResult(new ResponseObject("User already logged", data));
        }

        public async Task<IActionResult> Register(UserModel userModel)
        {
            await _userRepository.Add(userModel);

            return new OkObjectResult(new ResponseObject("User registered"));
        }

        public async Task<IActionResult> AuthorizeAdmin(string username)
        {
            var user = await GetUser(username);


            if (user.Role == Constants.AdminRole)
                return new OkResult();

            return new UnauthorizedResult();
        }

        public async Task<IActionResult> Authorize(string username)
        {
            var user = await GetUser(username);


            if (user == null)
                throw new ArgumentException(ErrorMessages.UserNotFound);



            if (user.Status == UserStatus.LoggedIn)
            {
                return new OkResult();
            }

            return new UnauthorizedResult();
        }

        private async Task<UserModel> GetUser(string username)
        {
            var user = await _userRepository.Get(username);

           return user;
        }
    }
}
