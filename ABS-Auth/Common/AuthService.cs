using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Data;
using Dapper;
using ABS_Auth.Helpers;
using ABS_Common.Constants;
using System.Linq;
using ABS_Common.ResponsesModels;
using ABS_Data.Data;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Abs.Common.Constants;
using System.Collections.Generic;
using Abs.Common.Constants.DbModels;
using ABS_Auth.Models;
using ABS.Data.DynamoDbRepository;

namespace ABS_Auth.Common
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<string, UserModel> _userRepository;
        private readonly IAmazonDynamoDB _connection;
        public AuthService(ABSContext context , IRepository<string , UserModel> repository )
        {
            _userRepository = repository;
            _connection = context.CreateConnection();
        }

        public async Task<IActionResult> Logout(string username)
        {
            var request = new UpdateItemRequest()
            {
                TableName = DbConstants.UsersTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { UserDbModel.Username, new AttributeValue { S = username } },
                },
                UpdateExpression = $"SET #status = :status",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":status", new AttributeValue { S = UserStatus.Registered.ToString() } },
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    {"#status" , UserDbModel.Status },
                },
            };

            await _connection.UpdateItemAsync(request);

            return new OkObjectResult(new ResponseObject("Logged out successfully"));
        }

        public async Task<IActionResult> Login(UserModel user, string secret)
        {
            
            try
            {
                var responseItem = await _userRepository.Update(user);

                string token = TokenService.GenerateJwtToken(user.Username, secret);


                bool isAdmin = responseItem.Roles == Constants.AdminRole;
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
            var responseItem = await GetUser(username);
            if (responseItem.Count == 0)
                throw new ArgumentException(ErrorMessages.UserNotFound);

            responseItem.TryGetValue(UserDbModel.Role, out var role);
            bool isAdmin = role.S == Constants.AdminRole;
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
            var responseItem = await GetUser(username);

            responseItem.TryGetValue(UserDbModel.Role, out var role);

            if (role.S == Constants.AdminRole)
                return new OkResult();

            return new UnauthorizedResult();
        }

        public async Task<IActionResult> Authorize(string username)
        {
            var responseItem = await GetUser(username);


            if (responseItem.Count == 0)
                throw new ArgumentException(ErrorMessages.UserNotFound);

            responseItem.TryGetValue(UserDbModel.Status, out var status);


            if (UserStatus.LoggedIn.ToString() == status.S)
            {
                return new OkResult();
            }

            return new UnauthorizedResult();
        }

        private async Task<Dictionary<string , AttributeValue>> GetUser(string username)
        {
            var request = new GetItemRequest()
            {
                TableName = DbConstants.UsersTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    {UserDbModel.Username, new AttributeValue {S = username} }
                }
            };

           return (await _connection.GetItemAsync(request)).Item;
        }
    }
}
