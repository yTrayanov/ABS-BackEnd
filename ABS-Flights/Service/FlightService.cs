using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS_Common.Enumerations;
using ABS_Common.ResponsesModels;
using ABS_Data.Data;
using ABS_Flights.Models;
using AirlineBookingSystem.Common.Extensions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Flights.Service
{
    public class FlightService : IFlightService
    {

        private IAmazonDynamoDB _connection;

        public FlightService(ABSContext contextService)
        {
            _connection = contextService.CreateConnection();
        }

        public async Task<IActionResult> CreateFlight(FlightBindingModel flightInfo)
        {
            var airlineId = await GetAirlineId(flightInfo.Airline);
            var originAirportId = await GetAirportId(flightInfo.OriginAirport);
            var destinationAirportId = await GetAirportId(flightInfo.DestinationAirport);

            var request = new PutItemRequest
            {
                TableName = DbConstants.TableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    {FlightDbModel.Id, new AttributeValue
                        {
                            S = FlightDbModel.Prefix + flightInfo.FlightNumber
                        }
                    },
                    {FlightDbModel.AirlineId, new AttributeValue()
                        {
                            S = airlineId
                        }
                    },
                    {FlightDbModel.Data , new AttributeValue()
                    {
                        M = new Dictionary<string, AttributeValue>()
                        {
                            {"DepartureDate", new AttributeValue() {S = flightInfo.DepartureDate.ToString() } },
                            {"LandingDate", new AttributeValue() {S = flightInfo.LandingDate.ToString() }  }
                        }
                    }},
                    {FlightDbModel.OriginAirportId , new AttributeValue {S = originAirportId } },
                    {FlightDbModel.DestinationAirportId , new AttributeValue {S = destinationAirportId } }
                },
                Expected = new Dictionary<string, ExpectedAttributeValue>()
                {
                    {FlightDbModel.Id, new ExpectedAttributeValue(false)}
                },
            };

            await _connection.PutItemAsync(request);

            return new OkObjectResult(new ResponseObject("Flight created"));
        }

        public async Task<IActionResult> FilterOneWayFlights(OneWayFlightModel flightInfo)
        {
            var flights = await FilterFlights(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            return new OkObjectResult(new ResponseObject("Flights found", flights.Select(f => new FlightBindingModel[] { f })));
        }

        public async Task<IActionResult> FilterTwoWayFlights(TwoWaySearchModel flightInfo)
        {
            var toDestinationFlights = await FilterFlights(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            if (toDestinationFlights == null)
                return new OkObjectResult(new ResponseObject("There are no fligths to destination on this date"));

            var returnFlights = await FilterFlights(flightInfo.DestinationAirport, flightInfo.OriginAirport, flightInfo.ReturnDate, flightInfo.MembersCount);
            if (returnFlights == null)
                return new OkObjectResult(new ResponseObject("There are no return flights"));

            var result = new List<FlightBindingModel[]>();

            for (int i = 0; i < toDestinationFlights.Count; i++)
            {
                for (int j = 0; j < returnFlights.Count; j++)
                {
                    var flights = new FlightBindingModel[] { toDestinationFlights[i], returnFlights[j] };
                    result.Add(flights);
                }
            }

            return new OkObjectResult(new ResponseObject("Flights found", result));
        }

        public async Task<IActionResult> GetAllFlights()
        {
            int flights = 0;

            return new OkObjectResult(new ResponseObject("Flights for all flights", flights));
        }

        public async Task<IActionResult> GetFlightById(string id)
        {

            var request = new GetItemRequest()
            {
                TableName = DbConstants.TableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                    {FlightDbModel.Id, new AttributeValue {S = FlightDbModel.Prefix + id}}
                }
            };

            var response = (await _connection.GetItemAsync(request)).Item;

            int flight = 0;

            return new OkObjectResult(new ResponseObject("Flight information", flight));
        }

        public async Task<IActionResult> GetMultipleFlights(string flightIds)
        {

            var ids = flightIds.Split(',').ToList();
            var mapAttValues = new Dictionary<string, AttributeValue>();

            int index = 0;
            foreach (var id in ids)
            {
                index++;
                mapAttValues.Add($":flightId{index}", new AttributeValue { S = FlightDbModel.Prefix + ids[index - 1] });
            }

            string stringIdentifiers = string.Join(",", mapAttValues.Keys);

            var request = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"(begins_with({FlightDbModel.Id} , :flightPrefix) AND PK IN ({stringIdentifiers}))" +
                $"OR(begins_with({SectionDbModel.Id}, :sectionPrefix) AND SK IN ({stringIdentifiers}))" +
                $"OR(begins_with({SeatDbModel.Id}, :seatPrefix) AND {SeatDbModel.FlightId} IN ({stringIdentifiers}))",

                ExpressionAttributeValues = new Dictionary<string, AttributeValue>(mapAttValues)
                {
                    {":flightPrefix" , new AttributeValue {S = FlightDbModel.Prefix } },
                    {":sectionPrefix" , new AttributeValue {S = SectionDbModel.Prefix } },
                    {":seatPrefix" , new AttributeValue {S = SeatDbModel.Prefix } },
                }
            };

            var responseItems = (await _connection.ScanAsync(request)).Items;

            var mappedFlights = new List<FlightBindingModel>();
            foreach(var flight in responseItems.Where(item => {
                                    item.TryGetValue(DbConstants.PK, out var id);
                                    return (id.S.StartsWith(FlightDbModel.Prefix));}))
            {
                flight.TryGetValue(FlightDbModel.Id, out var id);
                flight.TryGetValue(FlightDbModel.AirlineId, out var airlineId);
                flight.TryGetValue(FlightDbModel.Data, out var data);
                flight.TryGetValue(FlightDbModel.OriginAirportId, out var originAirportId);
                flight.TryGetValue(FlightDbModel.DestinationAirportId, out var destinationAirportId);

                string flightId = id.S.Replace(FlightDbModel.Prefix, "");
                mappedFlights.Add(new FlightBindingModel
                {
                    Id = flightId,
                    Airline = await GetAirlineName(airlineId.S),
                    OriginAirport = await GetAirportName(originAirportId.S),
                    DestinationAirport = await GetAirportName(destinationAirportId.S),
                    FlightNumber = flightId,

                });
            }

            var mappedSections = new List<Section>();
            foreach (var section in responseItems.Where(item => {
                item.TryGetValue(DbConstants.PK, out var id);
                return (id.S.StartsWith(SectionDbModel.Prefix));
            }))
            {
                section.TryGetValue(SectionDbModel.Id, out var id);
                section.TryGetValue(SectionDbModel.FlightId, out var flightId);
                section.TryGetValue(SectionDbModel.Data, out var data);

                data.M.TryGetValue("Rows", out var rows);
                data.M.TryGetValue("Columns", out var columns);
                data.M.TryGetValue("SeatClass", out var seatClass);

                mappedSections.Add(new Section
                {
                    Id = id.S,
                    FlightId = flightId.S.Replace(FlightDbModel.Prefix , ""),
                    Rows = int.Parse(rows.N),
                    Columns = int.Parse(columns.N),
                    SeatClass = seatClass.S == SeatClass.First.ToString() ? SeatClass.First : seatClass.S == SeatClass.Bussiness.ToString() ? SeatClass.Bussiness : SeatClass.Economy,
                }) ;
            }

            var mappedSeats = new List<Seat>();
            foreach (var seat in responseItems.Where(item => {
                item.TryGetValue(DbConstants.PK, out var id);
                return (id.S.StartsWith(SeatDbModel.Prefix));
            }))
            {
                seat.TryGetValue(SeatDbModel.Id, out var id);
                seat.TryGetValue(SeatDbModel.FlightId, out var flightId);
                seat.TryGetValue(SeatDbModel.SectionId, out var sectionId);
                seat.TryGetValue(SeatDbModel.Data, out var data);

                data.M.TryGetValue("Row", out var row);
                data.M.TryGetValue("Column", out var column);
                data.M.TryGetValue("SeatClass", out var seatClass);

                mappedSeats.Add(new Seat
                {
                    Id = id.S,
                    FlightId = flightId.S,
                    Row = int.Parse(row.N),
                    Column = int.Parse(column.N),
                    SeatClass = seatClass.S == SeatClass.First.ToString() ? SeatClass.First : seatClass.S == SeatClass.Bussiness.ToString() ? SeatClass.Bussiness : SeatClass.Economy,
                    SectionId = sectionId.S
                });
            }



            foreach (var section in mappedSections)
            {
                section.Seats = mappedSeats.Where(seat => seat.SectionId == section.Id).ToList();
            }

            foreach (var flight in mappedFlights)
            {
                flight.Sections = mappedSections.Where(section => section.FlightId == flight.Id).ToList();
            }

            return new OkObjectResult(new ResponseObject("Flights found", mappedFlights.OrderBy(f => ids.IndexOf(f.Id))));
        }

        private async Task<IList<FlightBindingModel>> FilterFlights(string originAirport, string destinationAirport, DateTime date, int membersCount)
        {

            var originAirportId = await GetAirportId(originAirport);
            var destinationAirportId =  await GetAirportId(destinationAirport);

            var request = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"begins_with({FlightDbModel.Id} , :prefix) " +
                                   $"and {FlightDbModel.OriginAirportId} = :originAirport " +
                                   $"and {FlightDbModel.DestinationAirportId} = :destinationAirport " +
                                   $"and begins_with( #data.DepartureDate , :departureDate )",

                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":originAirport" , new AttributeValue {S = originAirportId } },
                    {":destinationAirport" , new AttributeValue {S = destinationAirportId } },
                    {":prefix" , new AttributeValue {S = FlightDbModel.Prefix} },
                    {":departureDate", new AttributeValue {S = date.ToShortDateString()}},
                },

                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#data" , "Data" }
                }
                
            };

            var flightResponseItems = (await _connection.ScanAsync(request)).Items;
            var flights = new List<FlightBindingModel>();

            foreach (var item in flightResponseItems)
            {
                item.TryGetValue(FlightDbModel.Id, out var id);
                item.TryGetValue(FlightDbModel.AirlineId, out var airlineId);
                item.TryGetValue(FlightDbModel.Data, out var data);

                data.M.TryGetValue("DepartureDate", out var departureDate);
                data.M.TryGetValue("LandingDate", out var landingDate);

                string flightId = id.S.Replace(FlightDbModel.Prefix, "");

                flights.Add(new FlightBindingModel
                {
                    Id = flightId,
                    Airline = await GetAirlineName(airlineId.S),
                    OriginAirport = originAirport,
                    DestinationAirport = destinationAirport,
                    FlightNumber = flightId,
                    DepartureDate = DateTime.Parse(departureDate.S),
                    LandingDate = DateTime.Parse(landingDate.S),
                    
                });
            }

            var flightIds = flights.Select(f => $"{FlightDbModel.Prefix}" + f.FlightNumber).ToArray();


            var mapAttValues = new Dictionary<string, AttributeValue>();

            int index = 0;
            foreach (var id in flightIds)
            {
                index++;
                mapAttValues.Add($":flightId{index}", new AttributeValue { S = flightIds[index - 1]});
            }


            var sectionRequest = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"(begins_with(PK, :sectionPrefix) AND SK IN ({string.Join(",", mapAttValues.Keys)}) AND #data.AvailableSeats >= :membersCount)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>(mapAttValues) 
                {
                    {":sectionPrefix" , new AttributeValue {S = SectionDbModel.Prefix } },
                    {":membersCount", new AttributeValue{N = membersCount.ToString()} },
                },

                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#data" , "Data" }
                }
            };

            var sectionResponseItems = (await _connection.ScanAsync(sectionRequest)).Items;

            if (!sectionResponseItems.Any())
                return null;



            return flights;
        }

        private async Task<string> GetAirlineId(string name)
        {
            var airlineRequest = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"{AirlineDbModel.Name} = :name",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":name" , new AttributeValue {S = name } }
                }
            };

            var items = (await _connection.ScanAsync(airlineRequest)).Items;

            if (items.Count == 0)
            {
                throw new ArgumentException(ErrorMessages.AirlineNotFound);
            }

            items[0].TryGetValue(AirlineDbModel.Id , out var id);

            return id.S;
        }

        private async Task<string> GetAirportId(string name)
        {
            var airlineRequest = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"{AirportDbModel.Name} = :name",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":name" , new AttributeValue {S = name } }
                }
            };

            var items = (await _connection.ScanAsync(airlineRequest)).Items;

            if (items.Count == 0)
            {
                throw new ArgumentException(ErrorMessages.AirportNotFound);
            }

            items[0].TryGetValue(AirportDbModel.Id, out var id);

            return id.S;
        }

        private async Task<string> GetAirlineName(string id)
        {
            var airlineRequest = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"{AirlineDbModel.Id} = :id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":id" , new AttributeValue {S = id } }
                }
            };

            var items = (await _connection.ScanAsync(airlineRequest)).Items;

            if (items.Count == 0)
            {
                throw new ArgumentException(ErrorMessages.AirlineNotFound);
            }

            items[0].TryGetValue(AirlineDbModel.Name, out var name);

            return name.S;
        }
        private async Task<string> GetAirportName(string id)
        {
            var airlineRequest = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"{AirportDbModel.Id} = :id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":id" , new AttributeValue {S = id } }
                }
            };

            var items = (await _connection.ScanAsync(airlineRequest)).Items;

            if (items.Count == 0)
            {
                throw new ArgumentException(ErrorMessages.AirlineNotFound);
            }

            items[0].TryGetValue(AirportDbModel.Name, out var name);

            return name.S;
        }

    }
}
