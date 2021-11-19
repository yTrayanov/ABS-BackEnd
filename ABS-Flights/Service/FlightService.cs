using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS.Data.DynamoDbRepository;
using ABS_Common.Enumerations;
using ABS_Common.ResponsesModels;
using ABS_Data.Data;
using ABS_Flights.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
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
        private IRepository<string, FlightModel> _flightRepository;

        public FlightService(ABSContext contextService, IRepository<string , FlightModel> flightRepository)
        {
            _connection = contextService.CreateConnection();
            _flightRepository = flightRepository;
        }

        public async Task<IActionResult> CreateFlight(FlightModel flightInfo)
        {
            //var airlineId = await GetAirlineIdAsybc(flightInfo.Airline);
            //var originAirportId = await GetAirportIdAsync(flightInfo.OriginAirport);
            //var destinationAirportId = await GetAirportIdAsync(flightInfo.DestinationAirport);

            //var request = new PutItemRequest
            //{
            //    TableName = DbConstants.TableName,
            //    Item = new Dictionary<string, AttributeValue>()
            //    {
            //        {FlightDbModel.Id, new AttributeValue
            //            {
            //                S = FlightDbModel.Prefix + flightInfo.FlightNumber
            //            }
            //        },
            //        {FlightDbModel.AirlineId, new AttributeValue()
            //            {
            //                S = airlineId
            //            }
            //        },
            //        {FlightDbModel.Data , new AttributeValue()
            //        {
            //            M = new Dictionary<string, AttributeValue>()
            //            {
            //                {"DepartureDate", new AttributeValue() {S = flightInfo.DepartureDate.ToString() } },
            //                {"LandingDate", new AttributeValue() {S = flightInfo.LandingDate.ToString() }  }
            //            }
            //        }},
            //        {FlightDbModel.OriginAirportId , new AttributeValue {S = originAirportId } },
            //        {FlightDbModel.DestinationAirportId , new AttributeValue {S = destinationAirportId } }
            //    },
            //    Expected = new Dictionary<string, ExpectedAttributeValue>()
            //    {
            //        {FlightDbModel.Id, new ExpectedAttributeValue(false)}
            //    },
            //};

            //await _connection.PutItemAsync(request);

            await _flightRepository.Add(flightInfo);

            return new OkObjectResult(new ResponseObject("Flight created"));
        }

        public async Task<IActionResult> FilterOneWayFlights(OneWayFlightModel flightInfo)
        {
            var flights = await FilterFlightsAsync(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            return new OkObjectResult(new ResponseObject("Flights found", flights.Select(f => new FlightModel[] { f })));
        }

        public async Task<IActionResult> FilterTwoWayFlights(TwoWaySearchModel flightInfo)
        {
            var toDestinationFlights = await FilterFlightsAsync(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            if (toDestinationFlights == null)
                return new OkObjectResult(new ResponseObject("There are no fligths to destination on this date"));

            var returnFlights = await FilterFlightsAsync(flightInfo.DestinationAirport, flightInfo.OriginAirport, flightInfo.ReturnDate, flightInfo.MembersCount);
            if (returnFlights == null)
                return new OkObjectResult(new ResponseObject("There are no return flights"));

            var result = new List<FlightModel[]>();

            for (int i = 0; i < toDestinationFlights.Count; i++)
            {
                for (int j = 0; j < returnFlights.Count; j++)
                {
                    var flights = new FlightModel[] { toDestinationFlights[i], returnFlights[j] };
                    result.Add(flights);
                }
            }

            return new OkObjectResult(new ResponseObject("Flights found", result));
        }

        public async Task<IActionResult> GetAllFlights()
        {
            var request = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"begins_with({FlightDbModel.Id} , :prefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":prefix" , new AttributeValue {S = FlightDbModel.Prefix } }
                }
            };

            var responseItems = (await _connection.ScanAsync(request)).Items;
            var flights = await MapFlightsListAsync(responseItems);

            return new OkObjectResult(new ResponseObject("Flights for all flights", flights));
        }

        public async Task<IActionResult> GetFlightByIdAsync(string id)
        {

            var request = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"({FlightDbModel.Id} = :flightId)" +
                $"OR (begins_with({SectionDbModel.Id}, :sectionPrefix) AND {SectionDbModel.FlightId} = :flightId )" +
                $"OR (begins_with({SeatDbModel.Id}, :seatPrefix) AND {SeatDbModel.FlightId} = :flightId AND #data.IsBooked = :isBooked) " +
                $"OR (begins_with({TicketDbModel.Id}, :ticketPrefix) AND {TicketDbModel.FlightId} = :flightId)",

                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":flightId" , new AttributeValue {S = FlightDbModel.Prefix + id } },
                    {":sectionPrefix" , new AttributeValue {S = SectionDbModel.Prefix } },
                    {":seatPrefix", new AttributeValue {S = SeatDbModel.Prefix } },
                    {":ticketPrefix", new AttributeValue {S = TicketDbModel.Prefix} },
                    {":isBooked", new AttributeValue {BOOL = true} }
                },

                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#data" , SeatDbModel.Data }
                }
            };

            var responseItems = (await _connection.ScanAsync(request)).Items;

            var flight = await MapFlightAsync(responseItems.FirstOrDefault(item => 
            {
                item.TryGetValue(FlightDbModel.Id, out var flightId);
                return flightId.S.StartsWith(FlightDbModel.Prefix);
            } ));

            var sections =  MapSections(responseItems.Where(item =>
            {
                item.TryGetValue(DbConstants.PK, out var id);
                return (id.S.StartsWith(SectionDbModel.Prefix));
            }).ToList());


            var seats = new List<Seat>();
            foreach (var seat in responseItems.Where(item => {
                item.TryGetValue(DbConstants.PK, out var id);
                return (id.S.StartsWith(SeatDbModel.Prefix));
            }))
            {
                seat.TryGetValue(SeatDbModel.Id, out var seatId);
                seat.TryGetValue(SeatDbModel.FlightId, out var flightId);
                seat.TryGetValue(SeatDbModel.SectionId, out var sectionId);
                seat.TryGetValue(SeatDbModel.Data, out var data);

                data.M.TryGetValue("Row", out var row);
                data.M.TryGetValue("Column", out var column);
                data.M.TryGetValue("SeatClass", out var seatClass);
                data.M.TryGetValue("IsBooked", out var isBooked);

                seats.Add(new Seat
                {
                    Id = seatId.S,
                    FlightId = flightId.S,
                    Row = int.Parse(row.N),
                    Column = int.Parse(column.N),
                    SeatClass = seatClass.S == SeatClass.First.ToString() ? SeatClass.First : seatClass.S == SeatClass.Bussiness.ToString() ? SeatClass.Bussiness : SeatClass.Economy,
                    SectionId = sectionId.S,
                });
            }

            foreach (var item in responseItems.Where(item => {
                item.TryGetValue(DbConstants.PK, out var id);
                return (id.S.StartsWith(TicketDbModel.Prefix));
            }))
            {
                item.TryGetValue(TicketDbModel.SeatId, out var seatId);
                item.TryGetValue(TicketDbModel.UserId, out var username);
                item.TryGetValue(TicketDbModel.Data, out var data);

                data.M.TryGetValue("PassengerName", out var passangerName);

                var seat = seats.FirstOrDefault(s => s.Id == seatId.S);
                seat.PassengerName = passangerName.S;
                seat.Username = username.S;

            }


            foreach (var section in sections)
            {
                section.Seats = seats.Where(seat => seat.SectionId == section.Id).ToList();
            }

            flight.Sections = sections;


            return new OkObjectResult(new ResponseObject("Flight information", flight));
        }

        public async Task<IActionResult> GetMultipleFlightsAsync(string flightIds)
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

            var mappedFlights = await MapFlightsListAsync(responseItems.Where(item => {
                item.TryGetValue(DbConstants.PK, out var id);
                return (id.S.StartsWith(FlightDbModel.Prefix));
            }).ToList());


            var mappedSections = MapSections(responseItems.Where(item =>
            {
                item.TryGetValue(DbConstants.PK, out var id);
                return (id.S.StartsWith(SectionDbModel.Prefix));
            }).ToList());
            

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
                data.M.TryGetValue("IsBooked", out var isBooked);

                mappedSeats.Add(new Seat
                {
                    Id = id.S,
                    FlightId = flightId.S,
                    Row = int.Parse(row.N),
                    Column = int.Parse(column.N),
                    SeatClass = seatClass.S == SeatClass.First.ToString() ? SeatClass.First : seatClass.S == SeatClass.Bussiness.ToString() ? SeatClass.Bussiness : SeatClass.Economy,
                    SectionId = sectionId.S,
                    IsBooked = isBooked.BOOL,
                });
            }



            foreach (var section in mappedSections)
            {
                section.Seats = mappedSeats.Where(seat => seat.SectionId == section.Id).OrderBy(seat => seat.Row).ThenBy(seat => seat.Column).ToList();
            }

            foreach (var flight in mappedFlights)
            {
                flight.Sections = mappedSections.Where(section => section.FlightId == flight.Id).OrderBy(section => section.SeatClass).ToList();
            }

            return new OkObjectResult(new ResponseObject("Flights found", mappedFlights.OrderBy(f => ids.IndexOf(f.Id))));
        }

        private async Task<IList<FlightModel>> FilterFlightsAsync(string originAirport, string destinationAirport, DateTime date, int membersCount)
        {

            var originAirportId = await GetAirportIdAsync(originAirport);
            var destinationAirportId =  await GetAirportIdAsync(destinationAirport);

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
            var flights = new List<FlightModel>();

            foreach (var item in flightResponseItems)
            {
                item.TryGetValue(FlightDbModel.Id, out var id);
                item.TryGetValue(FlightDbModel.AirlineId, out var airlineId);
                item.TryGetValue(FlightDbModel.Data, out var data);

                data.M.TryGetValue("DepartureDate", out var departureDate);
                data.M.TryGetValue("LandingDate", out var landingDate);

                string flightId = id.S.Replace(FlightDbModel.Prefix, "");

                flights.Add(new FlightModel
                {
                    Id = flightId,
                    Airline = await GetAirlineNameAsync(airlineId.S),
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

        private async Task<string> GetAirlineIdAsybc(string name)
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

        private async Task<string> GetAirportIdAsync(string name)
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

        private async Task<string> GetAirlineNameAsync(string id)
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
        private async Task<string> GetAirportNameAsync(string id)
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


        private async Task<List<FlightModel>> MapFlightsListAsync(List<Dictionary<string, AttributeValue>> responseItems)
        {
            var mappedFlights = new List<FlightModel>();
            foreach (var flight in responseItems)
            {
                mappedFlights.Add(await MapFlightAsync(flight));
            }

            return mappedFlights;
        }

        private async Task<FlightModel> MapFlightAsync(Dictionary<string, AttributeValue> responseItem)
        {
            responseItem.TryGetValue(FlightDbModel.Id, out var id);
            responseItem.TryGetValue(FlightDbModel.AirlineId, out var airlineId);
            responseItem.TryGetValue(FlightDbModel.Data, out var data);
            responseItem.TryGetValue(FlightDbModel.OriginAirportId, out var originAirportId);
            responseItem.TryGetValue(FlightDbModel.DestinationAirportId, out var destinationAirportId);

            data.M.TryGetValue("DepartureDate", out var departureDate);
            data.M.TryGetValue("LandingDate", out var landingDate);

            string flightId = id.S.Replace(FlightDbModel.Prefix, "");

            return new FlightModel
            {
                Id = flightId,
                Airline = await GetAirlineNameAsync(airlineId.S),
                OriginAirport = await GetAirportNameAsync(originAirportId.S),
                DestinationAirport = await GetAirportNameAsync(destinationAirportId.S),
                FlightNumber = flightId,
                DepartureDate = DateTime.Parse(departureDate.S),
                LandingDate = DateTime.Parse(landingDate.S),

            };
        }

        private List<Section> MapSections(List<Dictionary<string, AttributeValue>> responseItems)
        {
            var mappedSections = new List<Section>();
            foreach (var section in responseItems)
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
                    FlightId = flightId.S.Replace(FlightDbModel.Prefix, ""),
                    Rows = int.Parse(rows.N),
                    Columns = int.Parse(columns.N),
                    SeatClass = seatClass.S == SeatClass.First.ToString() ? SeatClass.First : seatClass.S == SeatClass.Bussiness.ToString() ? SeatClass.Bussiness : SeatClass.Economy,
                });
            }

            return mappedSections;
        }

    }
}
