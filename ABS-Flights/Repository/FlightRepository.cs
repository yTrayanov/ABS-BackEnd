using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS.Data.DynamoDb;
using ABS.Data.DynamoDbRepository;
using ABS_Common.Enumerations;
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
            var data = item.GetInnerObjectData("Data");
            var id = item.GetString(FlightDbModel.Id).Replace(_PREFIX, "");
            var flight = new FlightModel
            {
                Id = id,
                FlightNumber = id,
                DepartureDate = DateTime.Parse(data.GetString("DepartureDate")),
                LandingDate = DateTime.Parse(data.GetString("LandingDate"))
            };
            return flight;
        }

        public async Task Add(FlightModel item)
        {
            var dynamoDbItem = ToDynamoDb(item);
            var airlineId = await GetAirlineIdAsync(item.Airline);
            var originAirportId = await GetAirportIdAsync(item.OriginAirport);
            var destinationAirportId = await GetAirportIdAsync(item.DestinationAirport);

            dynamoDbItem.AddGSI1(originAirportId);
            dynamoDbItem.AddGSI2(destinationAirportId);

            await _dynamoDbClient.PutItemAsync(GenerateFlightId(item.FlightNumber), airlineId, dynamoDbItem);
        }

        public Task Delete(string key)
        {
            throw new NotImplementedException();
        }

        public async Task<FlightModel> Get(string key)
        {

            var filterExpression = $"({FlightDbModel.Id} = :flightId)" +
            $"OR (begins_with({SectionDbModel.Id}, :sectionPrefix) AND {SectionDbModel.FlightId} = :flightId )" +
            $"OR (begins_with({SeatDbModel.Id}, :seatPrefix) AND {SeatDbModel.FlightId} = :flightId AND #data.IsBooked = :isBooked) " +
            $"OR (begins_with({TicketDbModel.Id}, :ticketPrefix) AND {TicketDbModel.FlightId} = :flightId)";

            var expressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":flightId" , new AttributeValue {S = FlightDbModel.Prefix + key } },
                    {":sectionPrefix" , new AttributeValue {S = SectionDbModel.Prefix } },
                    {":seatPrefix", new AttributeValue {S = SeatDbModel.Prefix } },
                    {":ticketPrefix", new AttributeValue {S = TicketDbModel.Prefix} },
                    {":isBooked", new AttributeValue {BOOL = true} }
                };

            var expressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#data" , SeatDbModel.Data }
                };

            var responseItems = await _dynamoDbClient.ScanItemsAsync(filterExpression, expressionAttributeValues , expressionAttributeNames);

            var flight = await MapFlightAsync(responseItems.FirstOrDefault(item => item.GetString(FlightDbModel.Id).StartsWith(_PREFIX)));

            var sections = MapSections(responseItems.Where(item => item.GetString(SectionDbModel.Id).StartsWith(SectionDbModel.Prefix)).ToList());


            var seats = new List<Seat>();
            foreach (var seat in responseItems.Where(item => item.GetString(SeatDbModel.Id).StartsWith(SeatDbModel.Prefix)))
            {
                var seatId = seat.GetString(SeatDbModel.Id);
                var flightId = seat.GetString(SeatDbModel.FlightId);
                var sectionId = seat.GetString(SeatDbModel.SectionId);
                var data = seat.GetInnerObjectData(SeatDbModel.Data);

                var row = data.GetInt32("Row");
                var column = data.GetInt32("Column");
                var seatClass = data.GetString("SeatClass");
                var isBooked = data.GetBoolean("IsBooked");

                seats.Add(new Seat
                {
                    Id = seatId,
                    FlightId = flightId,
                    Row = row,
                    Column = column,
                    SeatClass = seatClass == SeatClass.First.ToString() ? SeatClass.First : seatClass == SeatClass.Bussiness.ToString() ? SeatClass.Bussiness : SeatClass.Economy,
                    SectionId = sectionId,
                });
            }

            foreach (var item in responseItems.Where(item => item.GetString(TicketDbModel.Id).StartsWith(TicketDbModel.Prefix)))
            {
                var seatId = item.GetString(TicketDbModel.SeatId);
                var username = item.GetString(TicketDbModel.UserId);
                var data = item.GetInnerObjectData(TicketDbModel.Data);

                var passangerName = data.GetString("PassengerName");

                var seat = seats.FirstOrDefault(s => s.Id == seatId);
                seat.PassengerName = passangerName;
                seat.Username = username;

            }

            foreach (var section in sections)
            {
                section.Seats = seats.Where(seat => seat.SectionId == section.Id).ToList();
            }

            flight.Sections = sections;

            return flight;
        }

        public async Task<IList<FlightModel>> GetList(params string[] args)
        {

            var flights = new List<FlightModel>();
            var responseItems = new List<DynamoDBItem>();
            Dictionary<string, AttributeValue> expressionAttributeValues;
            string filterExpression;

            switch (args[0])
            {
                case "FilterFlights":
                    var originAirportId = await GetAirportIdAsync(args[1]);
                    var destinationAirportId = await GetAirportIdAsync(args[2]);
                    var date = args[3];


                    filterExpression = $"begins_with({FlightDbModel.Id} , :prefix) " +
                                       $"and {FlightDbModel.OriginAirportId} = :originAirport " +
                                       $"and {FlightDbModel.DestinationAirportId} = :destinationAirport " +
                                       $"and begins_with( #data.DepartureDate , :departureDate )";

                    expressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":originAirport" , new AttributeValue {S = originAirportId } },
                    {":destinationAirport" , new AttributeValue {S = destinationAirportId } },
                    {":prefix" , new AttributeValue {S = FlightDbModel.Prefix} },
                    {":departureDate", new AttributeValue {S = date}},
                };

                    var expressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#data" , "Data" }
                };

                    responseItems = await _dynamoDbClient.ScanItemsAsync(filterExpression, expressionAttributeValues, expressionAttributeNames);

                    flights = await MapFlightsListAsync(responseItems);

                    break;

                case "AllFlights":
                    filterExpression = $"begins_with({FlightDbModel.Id} , :prefix)";
                    expressionAttributeValues = new Dictionary<string, AttributeValue>()
                        {
                            {":prefix" , new AttributeValue {S = _PREFIX} }
                        };
                    responseItems = await _dynamoDbClient.ScanItemsAsync(filterExpression, expressionAttributeValues);

                    flights = await MapFlightsListAsync(responseItems);

                    break;


            }

            return flights;


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

            var items = (await _dynamoDbClient.ScanItemsAsync(filterExpression, expressionAttributeValues));

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

        private async Task<FlightModel> MapFlightAsync(DynamoDBItem item)
        {
            var flight = FromDynamoDb(item);

            var airlineId = item.GetString(FlightDbModel.AirlineId);
            flight.Airline = await GetAirlineNameAsync(airlineId);

            var originAirportId = item.GetString(FlightDbModel.OriginAirportId);
            flight.OriginAirport = await GetAirportNameAsync(originAirportId);

            var destinationAirportId = item.GetString(FlightDbModel.DestinationAirportId);
            flight.DestinationAirport = await GetAirportNameAsync(destinationAirportId);

            return flight;
        }

        private async Task<List<FlightModel>> MapFlightsListAsync(List<DynamoDBItem> responseItems)
        {
            var mappedFlights = new List<FlightModel>();
            foreach (var flight in responseItems)
            {
                mappedFlights.Add(await MapFlightAsync(flight));
            }

            return mappedFlights;
        }

        private List<Section> MapSections(List<DynamoDBItem> responseItems)
        {
            var mappedSections = new List<Section>();
            foreach (var section in responseItems)
            {
                var id = section.GetString(SectionDbModel.Id);
                var flightId = section.GetString(SectionDbModel.FlightId);
                var data = section.GetInnerObjectData(SectionDbModel.Data);

                var rows = data.GetInt32("Rows");
                var columns = data.GetInt32("Columns");
                var seatClass = data.GetString("SeatClass");

                mappedSections.Add(new Section
                {
                    Id = id,
                    FlightId = flightId.Replace(FlightDbModel.Prefix, ""),
                    Rows = rows,
                    Columns = columns,
                    SeatClass = seatClass == SeatClass.First.ToString() ? SeatClass.First : seatClass == SeatClass.Bussiness.ToString() ? SeatClass.Bussiness : SeatClass.Economy,
                });
            }

            return mappedSections;
        }
    }
}
