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
    public class AirportRepository : RepositoryBase<string, AirportModel>, IRepository<string, AirportModel>
    {
        public AirportRepository(IConfiguration configuration) : base(configuration)
        {
        }

        
        public async Task Add(AirportModel item)
        {
            await CheckIfAirportExists(item);
            await _dynamoDbClient.PutItemAsync(AirportDbModel.Prefix + Guid.NewGuid(), item.Name, ToDynamoDb(item));
        }

        public Task AddRange(ICollection<AirportModel> items)
        {
            throw new NotImplementedException();
        }

        public Task Delete(string key)
        {
            throw new NotImplementedException();
        }

        public Task<AirportModel> Get(string key)
        {
            throw new NotImplementedException();
        }

        public Task<IList<AirportModel>> GetList(params string[] args)
        {
            throw new NotImplementedException();
        }

        public Task<AirportModel> Update(AirportModel item)
        {
            throw new NotImplementedException();
        }

        protected override AirportModel FromDynamoDb(DynamoDBItem item)
        {
            throw new NotImplementedException();
        }

        protected override DynamoDBItem ToDynamoDb(AirportModel item)
        {
            var dynamoItem = new DynamoDBItem();

            return dynamoItem;
        }


        private async Task CheckIfAirportExists(AirportModel airport)
        {
            var filterExpression = $"{AirportDbModel.Name} = :name and begins_with({AirportDbModel.Id} , :prefix)";
            var expressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":name" , new AttributeValue() {S = airport.Name} },
                    {":prefix", new AttributeValue() {S = AirportDbModel.Prefix } }
                };

            var items = await _dynamoDbClient.ScanItemsAsync(filterExpression, expressionAttributeValues);

            if (items.Count > 0)
                throw new ArgumentException(ErrorMessages.AirportExists);
        }
    }
}
