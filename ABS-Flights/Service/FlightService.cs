using Abs.Common.Constants;
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
                    {DbConstants.PK, new AttributeValue
                        {
                            S = DbConstants.FlightPrefix + flightInfo.FlightNumber
                        }
                    },
                    {DbConstants.SK, new AttributeValue()
                        {
                            S = airlineId
                        }
                    },
                    {DbConstants.Data , new AttributeValue()
                    {
                        M = new Dictionary<string, AttributeValue>()
                        {
                            {"DepartureDate", new AttributeValue() {S = flightInfo.DepartureDate.ToString() } },
                            {"LandingDate", new AttributeValue() {S = flightInfo.LandingDate.ToString() }  }
                        }
                    }},
                    {DbConstants.GSI1 , new AttributeValue {S = originAirportId } },
                    {DbConstants.GSI2 , new AttributeValue {S = destinationAirportId } }
                },
                Expected = new Dictionary<string, ExpectedAttributeValue>()
                {
                    {DbConstants.PK, new ExpectedAttributeValue(false)}
                },
            };

            await _connection.PutItemAsync(request);

            return new OkObjectResult(new ResponseObject("Flight created"));
        }

        public async Task<IActionResult> FilterOneWayFlights(OneWayFlightModel flightInfo)
        {
            var flights = await FilterFlights(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            return new OkObjectResult(new ResponseObject("Flights found", flights.Select(f => new FilteredFlightModel[] { f })));
        }

        public async Task<IActionResult> FilterTwoWayFlights(TwoWaySearchModel flightInfo)
        {
            var toDestinationFlights = await FilterFlights(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            if (!toDestinationFlights.Any())
                return new OkObjectResult(new ResponseObject("There are no fligths to destination on this date"));

            var returnFlights = await FilterFlights(flightInfo.DestinationAirport, flightInfo.OriginAirport, flightInfo.ReturnDate, flightInfo.MembersCount);
            if (!returnFlights.Any())
                return new OkObjectResult(new ResponseObject("There are no return flights"));

            var result = new List<FilteredFlightModel[]>();

            for (int i = 0; i < toDestinationFlights.Count; i++)
            {
                for (int j = 0; j < returnFlights.Count; j++)
                {
                    var flights = new FilteredFlightModel[] { toDestinationFlights[i], returnFlights[j] };
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
            int flight = 0;

            return new OkObjectResult(new ResponseObject("Flight information", flight));
        }

        public async Task<IActionResult> GetMultipleFlights(string flightIds)
        {

            List<int> ids = flightIds.Split(',').Select(int.Parse).ToList();

            var data = new List<FlightIdModel>();
            foreach (var id in ids)
            {
                data.Add(new FlightIdModel { FlightId = id });
            }

            
            //using (var multi = await _connection.QueryMultipleAsync(query, new { FlightIds = data.ToDataTable().AsTableValuedParameter("FlightIdList") }))
            //{

            //    var flights = (await multi.ReadAsync<FlightWithSectionsModel>()).OrderBy(f => ids.IndexOf(f.Id)).ToList();

            //    if (flights.Count < ids.Count)
            //    {
            //        return new BadRequestObjectResult(new ResponseObject("Could not find all selected flights"));
            //    }
            //    var sections = (await multi.ReadAsync<Section>()).ToList();
            //    var seats = (await multi.ReadAsync<Seat>()).ToList();

            //    foreach (var flight in flights)
            //    {
            //        flight.Sections = sections.Where(section => section.FlightId == flight.Id).OrderBy(s => s.SeatClass).ToList();

            //        foreach (var section in flight.Sections)
            //        {
            //            section.Seats = seats.Where(seat => seat.SectionId == section.Id).ToList();
            //        }
            //    }


            //    return new OkObjectResult(new ResponseObject("Flights found", flights));
            //}

            throw new NotImplementedException();
        }

        private async Task<IList<FilteredFlightModel>> FilterFlights(string originAirport, string destinationAirport, DateTime departureDate, int membersCount)
        {
            string query = "EXEC dbo.usp_FilterFlights_Select @OriginAirport, @DestinationAirport, @DepartureDate, @MembersCount";

            var flights = new List<FilteredFlightModel>();

            return flights;
        }

        private async Task<string> GetAirlineId(string name)
        {
            var airlineRequest = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"{DbConstants.SK} = :name",
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

            items[0].TryGetValue(DbConstants.PK , out var id);

            return id.S;
        }

        private async Task<string> GetAirportId(string name)
        {
            var airlineRequest = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"{DbConstants.SK} = :name",
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

            items[0].TryGetValue(DbConstants.PK, out var id);

            return id.S;
        }
    }
}
