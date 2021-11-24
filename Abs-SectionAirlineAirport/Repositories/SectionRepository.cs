using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS.Data.DynamoDb;
using ABS.Data.DynamoDbRepository;
using ABS_Common.Enumerations;
using Abs_SectionAirlineAirport.Models;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abs_SectionAirlineAirport.Repositories
{
    public class SectionRepository : RepositoryBase<string, SectionModel>, IRepository<string, SectionModel>
    {
        public SectionRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override SectionModel FromDynamoDb(DynamoDBItem item)
        {
            throw new NotImplementedException();
        }

        protected override DynamoDBItem ToDynamoDb(SectionModel item)
        {
            var dynamoItem = new DynamoDBItem();
            dynamoItem.AddPK(Guid.NewGuid().ToString(), SectionDbModel.Prefix);
            dynamoItem.AddSK(item.FlightNumber, FlightDbModel.Prefix);

            var innerData = new DynamoDBItem();
            innerData.AddNumber("Rows", item.Rows);
            innerData.AddNumber("Columns", item.Columns);
            innerData.AddNumber("AvailableSeats", (item.Rows * item.Columns));
            innerData.AddString("SeatClass", item.SeatClass.ToString());

            dynamoItem.AddData(innerData);

            return dynamoItem;
        }

        public async Task Add(SectionModel item)
        {
            var flightNumber = FlightDbModel.Prefix + item.FlightNumber;

            await CheckIfFlightExists(flightNumber);

            await CheckIfFlightHasSeatClass(item.SeatClass, flightNumber);

            var sectionId = SectionDbModel.Prefix + Guid.NewGuid();

            var itemsToCreate = new List<DynamoDBItem>();
            itemsToCreate.Add(ToDynamoDb(item));


            for (int row = 0; row < item.Rows; row++)
            {
                for (int col = 0; col < item.Columns; col++)
                {
                    var seatItem = new DynamoDBItem();
                    seatItem.AddPK(Guid.NewGuid().ToString(), SeatDbModel.Prefix);
                    seatItem.AddSK(item.FlightNumber, FlightDbModel.Prefix);
                    seatItem.AddGSI1(sectionId);

                    var innerData = new DynamoDBItem();
                    innerData.AddBoolean("IsBooked", false);
                    innerData.AddNumber("Row", row + 1);
                    innerData.AddNumber("Column", col + 1);
                    innerData.AddString("SeatClass", item.SeatClass.ToString());

                    seatItem.AddData(innerData);

                    itemsToCreate.Add(seatItem);
                }
            }


            await _dynamoDbClient.BatchAddItemsAsync(itemsToCreate);

            //int length = putRequests.Count;
            //int index = 0;

            //for (int i = 1; i <= Math.Ceiling(length / 25f); i++)
            //{
            //    var requestItems = new Dictionary<string, List<WriteRequest>>();
            //    requestItems.Add(DbConstants.TableName, new List<WriteRequest>());

            //    for (int j = 0; j < (length > 25 ? 25 : length); j++)
            //    {
            //        requestItems[DbConstants.TableName].Add(new WriteRequest(putRequests[j + index]));
            //    }


            //    index += 25;
            //    length -= 25;

            //    var batchRequest = new BatchWriteItemRequest()
            //    {
            //        RequestItems = requestItems
            //    };

            //    await _connection.BatchWriteItemAsync(batchRequest);
            //}
        }

        public Task Delete(string key)
        {
            throw new NotImplementedException();
        }

        public Task<SectionModel> Get(string key)
        {
            throw new NotImplementedException();
        }

        public Task<IList<SectionModel>> GetList(params string[] args)
        {
            throw new NotImplementedException();
        }

        public Task<SectionModel> Update(SectionModel item)
        {
            throw new NotImplementedException();
        }



        private async Task CheckIfFlightExists(string flightNumber)
        {
            var filterExpression = $"{FlightDbModel.Id} = :flightNumber";
            var expressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":flightNumber" , new AttributeValue() {S = flightNumber} },
                };

            var items = await _dynamoDbClient.ScanItemsAsync(filterExpression , expressionAttributeValues);

            if (items.Count == 0)
                throw new ArgumentException(ErrorMessages.FlightNotFound);
        }

        private async Task CheckIfFlightHasSeatClass(SeatClass seatClass, string flightNumber)
        {
            var filterExpression = $"{FlightDbModel.Id} = :flightNumber";
            var expressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":flightNumber" , new AttributeValue {S = flightNumber } },
                };

            var items = await _dynamoDbClient.ScanItemsAsync(filterExpression , expressionAttributeValues);

            foreach (var section in items)
            {
                var data = section.GetInnerObjectData(FlightDbModel.Data);
                var sectionSeatClass = data.GetString("SeatClass");

                if (sectionSeatClass == seatClass.ToString())
                {
                    throw new ArgumentException(ErrorMessages.SeatClassExists);
                }
            }
        }
    }
}
