using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS.Data.DynamoDb;
using ABS.Data.DynamoDbRepository;
using Abs_SectionAirlineAirport.Models;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abs_SectionAirlineAirport.Repositories
{
    public class AirlineRepository : RepositoryBase<string, AirlineModel>, IRepository<string, AirlineModel>
    {
        public AirlineRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override AirlineModel FromDynamoDb(DynamoDBItem item)
        {
            throw new NotImplementedException();
        }

        protected override DynamoDBItem ToDynamoDb(AirlineModel item)
        {
            var dynamoDbItem = new DynamoDBItem();

            return dynamoDbItem;
        }

        public async Task Add(AirlineModel item)
        {
            await CheckIfAirlineExists(item);
            await _dynamoDbClient.PutItemAsync(AirlineDbModel.Prefix + Guid.NewGuid(), item.Name , ToDynamoDb(item) );
        }

        public Task Delete(string key)
        {
            throw new NotImplementedException();
        }

        public Task<AirlineModel> Get(string key)
        {
            throw new NotImplementedException();
        }

        public Task<IList<AirlineModel>> GetList(params string[] args)
        {
            throw new NotImplementedException();
        }

        public Task<AirlineModel> Update(AirlineModel item)
        {
            throw new NotImplementedException();
        }

        private async Task CheckIfAirlineExists(AirlineModel airline)
        {
            var filterExpression = $"{AirlineDbModel.Name} = :airlineName and begins_with({AirlineDbModel.Id} , :airlinePrefix)";
            var expressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":airlineName" , new AttributeValue() {S = airline.Name} },
                    {":airlinePrefix", new AttributeValue() {S = AirlineDbModel.Prefix } }
                };

            var items = await _dynamoDbClient.ScanItemsAsync(filterExpression, expressionAttributeValues);

            if (items.Count > 0)
                throw new ArgumentException(ErrorMessages.AirlineExists);
        }

        public Task AddRange(ICollection<AirlineModel> items)
        {
            throw new NotImplementedException();
        }

    }
}
