using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS.Data.DynamoDb;
using ABS.Data.DynamoDbRepository;
using ABS_Common.Enumerations;
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
            var ticketId =  item.GetString(TicketDbModel.Id);
            var flightId = item.GetString(TicketDbModel.FlightId);
            var data = item.GetInnerObjectData(TicketDbModel.Data);
            var seatId = item.GetString(TicketDbModel.SeatId);
            var passangerName = data.GetString("PassengerName");
            

            return new TicketModel
            {
                FlightId = flightId,
                Id = ticketId,
                PassengerName = passangerName,
                SeatId = seatId,
            };
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
            var seatsForBooking = items.Select(i => new SeatModel() { Id = i.SeatId, FlightId = i.FlightId, SectionId = i.SectionId}).ToList();

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

        public async Task<IList<TicketModel>> GetList(params string[] args)
        {
            var username = args[0];
            var filterExpression = $"begins_with({TicketDbModel.Id},:ticketPrefix) AND {TicketDbModel.UserId} = :userId ";
            var expressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":ticketPrefix", new AttributeValue {S = TicketDbModel.Prefix } },
                    {":userId", new AttributeValue {S = username} },
                };

            var ticketItems = await _dynamoDbClient.ScanItemsAsync(filterExpression, expressionAttributeValues);

            var tickets = new List<TicketModel>();
            foreach (var item in ticketItems)
            {
                var ticket = FromDynamoDb(item);
                ticket.Username = username;
                tickets.Add(FromDynamoDb(item));
            }

            await MapTicketFlights(tickets);

            return tickets;
        }

        public Task<TicketModel> Update(TicketModel item)
        {
            throw new System.NotImplementedException();
        }

        private async Task BookSeats(ICollection<SeatModel> seats)
        {
            var seatItems = new List<DynamoDBItem>();

            foreach (var seat in seats)
            {
                var item = new DynamoDBItem();
                item.AddPK(seat.Id);
                item.AddSK(FlightDbModel.Prefix + seat.FlightId);

                seatItems.Add(item);
            }

            var updateExpression = $"SET #data.IsBooked = :isBooked";
            var expressionAttributeNames = new Dictionary<string, string>
            {
                {"#data" , SeatDbModel.Data }
            };
            var expressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":isBooked", new AttributeValue {BOOL = true} },
            };

            await _dynamoDbClient.BatchUpdateItemAsync(seatItems, updateExpression, expressionAttributeValues, expressionAttributeNames);

            var objects = seats.Select(s => new { s.SectionId , s.FlightId } ).Distinct().ToList();

            foreach (var obj in objects)
            {
                var key = new Dictionary<string, AttributeValue>()
                {
                    {SectionDbModel.Id , new AttributeValue(obj.SectionId)},
                    {SectionDbModel.FlightId , new AttributeValue(FlightDbModel.Prefix + obj.FlightId) }
                };
                updateExpression = $"ADD #data.AvailableSeats :bookedSeatCount";
                expressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":bookedSeatCount", new AttributeValue {N = (-seats.Where(s => s.SectionId == obj.SectionId).Count()).ToString() } }
                };

                await _dynamoDbClient.UpdateItemAsync(key, updateExpression, null, expressionAttributeValues, expressionAttributeNames);
            }


        }

        private async Task MapTicketFlights(IList<TicketModel> tickets)
        {
            var flightIds = tickets.Select(t => t.FlightId).Distinct().ToList();

            var mapAttValues = new Dictionary<string, AttributeValue>();

            int index = 0;
            foreach (var id in flightIds)
            {
                mapAttValues.Add($":flightId{index}", new AttributeValue { S = flightIds[index] });

                index++;
            }

            index = 0;
            var seatIds = tickets.Select(t => t.SeatId).ToList();
            foreach (var item in seatIds)
            {
                mapAttValues.Add($":seatId{index}", new AttributeValue { S = seatIds[index] });
                index++;
            }

            string stringIdentifiers = string.Join(",", mapAttValues.Keys);

            var filterExpression = $"(begins_with({FlightDbModel.Id} , :flightPrefix) AND {FlightDbModel.Id} IN ({stringIdentifiers}))" +
                            $"OR(begins_with({SeatDbModel.Id}, :seatPrefix) AND {SeatDbModel.Id} IN ({stringIdentifiers}))";

                var expressionAttributeValues = new Dictionary<string, AttributeValue>(mapAttValues)
                {
                    {":flightPrefix" , new AttributeValue {S = FlightDbModel.Prefix } },
                    {":seatPrefix" , new AttributeValue {S = SeatDbModel.Prefix } },
                };

            var responseItems = await _dynamoDbClient.ScanItemsAsync(filterExpression , expressionAttributeValues);
            var flights = new List<Flight>();
            foreach (var item in responseItems.Where(item => item.GetString(DynamoDBConstants.PK).StartsWith(FlightDbModel.Prefix)))
            {
                var flightId = item.GetString(FlightDbModel.Id);
                var airlineId = item.GetString(FlightDbModel.AirlineId);
                var data = item.GetInnerObjectData(FlightDbModel.Data);
                var originAirportId = item.GetString(FlightDbModel.OriginAirportId);
                var destinationAirportId = item.GetString(FlightDbModel.DestinationAirportId);

                var departureDate = data.GetString("DepartureDate");
                var landingDate = data.GetString("LandingDate");

                var flight = new Flight()
                {
                    Id = flightId,
                    Airline = await GetAirlineNameAsync(airlineId),
                    OriginAirport = await GetAirportNameAsync(originAirportId),
                    DestinationAirport = await GetAirportNameAsync(originAirportId),
                    FlightNumber = flightId.Replace(FlightDbModel.Prefix, ""),
                    DepartureDate = DateTime.Parse(departureDate),
                    LandingDate = DateTime.Parse(landingDate),
                };

                flights.Add(flight);
            }

            foreach (var ticket in tickets)
            {
                ticket.Flight = flights.FirstOrDefault(f => f.Id == ticket.FlightId);
            }


            foreach (var item in responseItems.Where(item => item.GetString(DynamoDBConstants.PK).StartsWith(SeatDbModel.Prefix)))
            {

                var seatId  = item.GetString(SeatDbModel.Id);
                var flightId = item.GetString(SeatDbModel.FlightId);
                var data = item.GetInnerObjectData(SeatDbModel.Data);

                var row = data.GetInt32("Row");
                var column = data.GetInt32("Column");
                var seatClass = data.GetString("SeatClass");
                bool isBooked = data.GetBoolean("IsBooked");

                var seat = new SeatModel()
                {
                    Id = seatId,
                    FlightId = flightId,
                    Row = row,
                    Column = column,
                    SeatClass = seatClass == SeatClass.First.ToString() ? SeatClass.First : seatClass == SeatClass.Bussiness.ToString() ? SeatClass.Bussiness : SeatClass.Economy,
                    IsBooked = isBooked,
                };

                tickets.FirstOrDefault(t => t.SeatId == seatId).Seat = seat;

            }
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
