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

namespace ABS_Auth.Common
{
    public class AuthService : IAuthService
    {
        private readonly IAmazonDynamoDB _connection;
        public AuthService(ABSContext context)
        {
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

        public async Task<IActionResult> Login(string username, string password, string secret)
        {
            var request = new UpdateItemRequest()
            {
                TableName = DbConstants.UsersTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { UserDbModel.Username, new AttributeValue { S = username } },
                },
                UpdateExpression = $"SET #status = :status",
                ConditionExpression = $"#password = :password",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":status", new AttributeValue { S = UserStatus.LoggedIn.ToString() } },
                    {":password" , new AttributeValue {S = password } },
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    {"#status" , UserDbModel.Status },
                    {"#password", UserDbModel.Password }
                },
                ReturnValues = new ReturnValue("ALL_NEW"),
            };

            try
            {
                var responseAttributes = (await _connection.UpdateItemAsync(request)).Attributes;

                string token = TokenService.GenerateJwtToken(username, secret);
                responseAttributes.TryGetValue(UserDbModel.Role, out var role);

                bool isAdmin = role.S == Constants.AdminRole;
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

        public async Task<IActionResult> Register(string username, string password, string email)
        {
            var request = new PutItemRequest()
            {
                TableName = DbConstants.UsersTableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {UserDbModel.Username, new AttributeValue {S = username } },
                    {UserDbModel.Email , new AttributeValue {S = email } },
                    {UserDbModel.Password , new AttributeValue {S = password} },
                    {UserDbModel.Status, new AttributeValue {S = UserStatus.Registered.ToString()} },
                    {UserDbModel.Role , new AttributeValue {S = "User" } }
                },
                Expected = new Dictionary<string, ExpectedAttributeValue>
                {
                    {UserDbModel.Username , new ExpectedAttributeValue(false) }
                }

            };

            try
            {
                await _connection.PutItemAsync(request);
            }
            catch (ConditionalCheckFailedException)
            {
                throw new ArgumentException(ErrorMessages.UsernameExists);
            }


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
