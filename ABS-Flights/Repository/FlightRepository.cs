using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS.Data.DynamoDb;
using ABS.Data.DynamoDbRepository;
using ABS_Flights.Models;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;

namespace ABS_Flights.Repository
{
    public class FlightRepository : RepositoryBase<string, FlightModel>, IRepository<string, FlightModel>
    {
        private const string _PREFIX = "FLIGHT#";
        public FlightRepository(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override DynamoDBItem ToDynamoDb(FlightModel item)
        {
            var flight = new DynamoDBItem();
            var data = new Dictionary<string, AttributeValue>();

            data.Add("LandingDate", new AttributeValue(item.LandingDate.ToString()));
            data.Add("DepartureDate", new AttributeValue(item.DepartureDate.ToString()));

            flight.AddData(data);

            return flight;
        }


        protected override FlightModel FromDynamoDb(DynamoDBItem item)
        {
            throw new NotImplementedException();
        }

        public async Task Add(FlightModel item)
        {
            var dynamoDbItem = ToDynamoDb(item);
            var airlineId = await GetAirlineIdAsync(item.Airline);
            var originAirportId = await GetAirportIdAsync(item.OriginAirport);
            var destinationAirportId = await GetAirportIdAsync(item.DestinationAirport);

            dynamoDbItem.AddGSI1(originAirportId);
            dynamoDbItem.AddGSI2(destinationAirportId);

            await _dynamoDbClient.PutItemAsync(GenerateFlightId(item.FlightNumber), airlineId , dynamoDbItem);
        }

        public Task Delete(string key)
        {
            throw new NotImplementedException();
        }

        public Task<FlightModel> Get(string key)
        {
            throw new NotImplementedException();
        }

        public Task<IList<FlightModel>> GetList()
        {
            throw new NotImplementedException();
        }

        public Task Update(FlightModel item)
        {
            throw new NotImplementedException();
        }


        private string GenerateFlightId(string flightNumber)
        {
            return _PREFIX + flightNumber;
        }
        private async Task<string> GetAirlineIdAsync(string name)
        {
            var filterExpression = $"{AirlineDbModel.Name} = :name";
            var expressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":name" , new AttributeValue {S = name } }
                };

            var items = (await _dynamoDbClient.ScanItemsAsync(filterExpression , expressionAttributeValues));

            if (items.Count == 0)
            {
                throw new ArgumentException(ErrorMessages.AirlineNotFound);
            }

            return items[0].GetString(AirlineDbModel.Id);

        }

        private async Task<string> GetAirportIdAsync(string name)
        {
            string filterExpression = $"{AirportDbModel.Name} = :name";
            var expressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":name" , new AttributeValue {S = name } }
                };

            var items = (await _dynamoDbClient.ScanItemsAsync(filterExpression, expressionAttributeValues));

            if (items.Count == 0)
            {
                throw new ArgumentException(ErrorMessages.AirportNotFound);
            }

            return items[0].GetString(AirportDbModel.Id);

        }

        private async Task<string> GetAirlineNameAsync(string id)
        {
            var filterExpression = $"{AirlineDbModel.Id} = :id";
            var expressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":id" , new AttributeValue {S = id } }
                };


            var items = (await _dynamoDbClient.ScanItemsAsync(filterExpression, expressionAttributeValues));

            if (items.Count == 0)
            {
                throw new ArgumentException(ErrorMessages.AirlineNotFound);
            }

            return items[0].GetString(AirlineDbModel.Name);
        }
        private async Task<string> GetAirportNameAsync(string id)
        {
            string filterExpression = $"{AirportDbModel.Id} = :id";
            var expressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":id" , new AttributeValue {S = id } }
                };


            var items = (await _dynamoDbClient.ScanItemsAsync(filterExpression, expressionAttributeValues));

            if (items.Count == 0)
            {
                throw new ArgumentException(ErrorMessages.AirlineNotFound);
            }

            return items[0].GetString(AirportDbModel.Name);

        }
    }
}
