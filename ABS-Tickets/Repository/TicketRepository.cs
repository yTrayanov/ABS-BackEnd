using Abs.Common.Constants.DbModels;
using ABS.Data.DynamoDb;
using ABS.Data.DynamoDbRepository;
using ABS_Tickets.Models;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Tickets.Repository
{
    public class TicketRepository : RepositoryBase<string, TicketModel>, IRepository<string, TicketModel>
    {
        public TicketRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override TicketModel FromDynamoDb(DynamoDBItem item)
        {
            throw new System.NotImplementedException();
        }

        protected override DynamoDBItem ToDynamoDb(TicketModel item)
        {
            var dynamoItem = new DynamoDBItem();
            dynamoItem.AddPK(Guid.NewGuid().ToString(), TicketDbModel.Prefix);
            dynamoItem.AddSK(item.Username);
            dynamoItem.AddGSI1(FlightDbModel.Prefix + item.FlightId);
            dynamoItem.AddGSI2(item.SeatId);

            var innerData = new DynamoDBItem();
            innerData.AddString("PassengerName", item.PassengerName);

            dynamoItem.AddData(innerData);

            return dynamoItem;
        }

        public Task Add(TicketModel item)
        {
            throw new System.NotImplementedException();
        }

        public async Task AddRange(ICollection<TicketModel> items)
        {
            var seatsForBooking = items.Select(i => new SeatModel() { Id = i.SeatId, FlightId = i.FlightId }).ToList();

            await BookSeats(seatsForBooking);

            var itemsToCreate = new List<DynamoDBItem>();

            foreach (var item in items)
            {
                itemsToCreate.Add(ToDynamoDb(item));
            }

            await _dynamoDbClient.BatchAddItemsAsync(itemsToCreate);
        }

        public Task Delete(string key)
        {
            throw new System.NotImplementedException();
        }

        public Task<TicketModel> Get(string key)
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<TicketModel>> GetList(params string[] args)
        {
            throw new System.NotImplementedException();
        }

        public Task<TicketModel> Update(TicketModel item)
        {
            throw new System.NotImplementedException();
        }

        private async Task BookSeats(ICollection<SeatModel> seats)
        {
            var dynamoItems = new List<DynamoDBItem>();

            foreach (var seat in seats)
            {
                var item = new DynamoDBItem();
                item.AddPK(seat.Id);
                item.AddSK(FlightDbModel.Prefix + seat.FlightId);

                dynamoItems.Add(item);
            }

            var updateEpression = $"SET #data.IsBooked = :isBooked";
            var expressionAttributeNames = new Dictionary<string, string>
            {
                {"#data" , SeatDbModel.Data }
            };
            var expressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":isBooked", new AttributeValue {BOOL = true} },
            };

            await _dynamoDbClient.BatchUpdateItemAsync(dynamoItems, updateEpression, expressionAttributeValues, expressionAttributeNames);
        }
    }
}
