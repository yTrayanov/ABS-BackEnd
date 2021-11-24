using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS.Data.DynamoDb;
using ABS.Data.DynamoDBHelpers;
using ABS.Data.DynamoDbRepository;
using ABS_Auth.Helpers;
using ABS_Auth.Models;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Auth.Repository
{
    public class UserRepository : RepositoryBase<string, UserModel>, IRepository<string, UserModel>
    {
        public UserRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override UserModel FromDynamoDb(DynamoDBItem item)
        {
            var user = new UserModel
            {
                Username = item.GetString(UserDbModel.Username),
                Password = item.GetString(UserDbModel.Password),
                Roles = item.GetString(UserDbModel.Role),
                Email = item.GetString(UserDbModel.Email),
                Status = ConvertUserStatus(item.GetString(UserDbModel.Status)),
            };

            return user;
        }

        protected override DynamoDBItem ToDynamoDb(UserModel item)
        {
            var dynamoItem = new DynamoDBItem();
            dynamoItem.AddString(UserDbModel.Password, item.Password);
            dynamoItem.AddString(UserDbModel.Status, item.Status.ToString());
            dynamoItem.AddString(UserDbModel.Role, item.Roles);
            dynamoItem.AddString(UserDbModel.Email, item.Email);

            return dynamoItem;
        }
        public async Task Add(UserModel item)
        {

            var dynamoItem = ToDynamoDb(item);

            try
            {
                await _dynamoDbClient.PutItemAsync(item.Username, null, dynamoItem);
            }
            catch (ConditionalCheckFailedException)
            {
                throw new ArgumentException(ErrorMessages.UsernameExists);
            }
        }

        public Task Delete(string key)
        {
            throw new NotImplementedException();
        }

        public Task<UserModel> Get(string key)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserModel>> GetList(params string[] args)
        {
            throw new NotImplementedException();
        }

        public async Task<UserModel> Update(UserModel item)
        {
            Dictionary<string, AttributeValue> key;
            string updateExpression;
            Dictionary<string, AttributeValue> expressionAttributeValues;
            Dictionary<string, string> expressionAttributeNames;
            string conditionExpression;
            DynamoDBItem response;


            if (item.Password != null)
            {
                key = new Dictionary<string, AttributeValue>
                {
                    { UserDbModel.Username, new AttributeValue { S = item.Username } },
                };
                updateExpression = $"SET #status = :status";
                conditionExpression = $"#password = :password";
                expressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":status", new AttributeValue { S = item.Status.ToString() } },
                    {":password" , new AttributeValue {S = item.Password } },
                };
                expressionAttributeNames = new Dictionary<string, string>
                {
                    {"#status" , UserDbModel.Status },
                    {"#password", UserDbModel.Password }
                };
                var returnValues = UpdateReturnValues.ALL_NEW;
                response = await _dynamoDbClient.UpdateItemAsync(key, updateExpression, conditionExpression, expressionAttributeValues, expressionAttributeNames, returnValues);

                return FromDynamoDb(response);
            }
            else
            {
                key = new Dictionary<string, AttributeValue>
                {
                    { UserDbModel.Username, new AttributeValue { S = item.Username } },
                };
                updateExpression = $"SET #status = :status";
                conditionExpression = $"#status <> :status";
                expressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":status", new AttributeValue { S = item.Status.ToString() } },
                };
                expressionAttributeNames = new Dictionary<string, string>
                {
                    {"#status" , UserDbModel.Status },
                };

                response = await _dynamoDbClient.UpdateItemAsync(key, updateExpression, conditionExpression, expressionAttributeValues, expressionAttributeNames);

                return null;
            }
        }

        private UserStatus ConvertUserStatus(string status)
        {
            return status == UserStatus.Registered.ToString() ? UserStatus.Registered : UserStatus.LoggedIn;
        }


    }
}
