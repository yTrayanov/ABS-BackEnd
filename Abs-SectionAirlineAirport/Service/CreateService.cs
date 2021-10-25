using Abs.Common.Constants;
using ABS_Common.Enumerations;
using ABS_Common.ResponsesModels;
using ABS_Data.Data;
using Abs_SectionAirlineAirport.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abs_SectionAirlineAirport.Service
{
    public class CreateService : ICreateService
    {


        private IAmazonDynamoDB _connection;

        public CreateService(ABSContext contextService)
        {
            _connection = contextService.CreateConnection();
        }

        public async Task<IActionResult> CreateAirline(AirlineModel airlineInfo)
        {

            await CheckIfAirlineExists(airlineInfo.Name);

            var request = new PutItemRequest
            {
                TableName = DbConstants.TableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    {DbConstants.PK, new AttributeValue
                        {
                            S = DbConstants.AirlinePrefix + Guid.NewGuid()
                        }
                    },

                    {DbConstants.SK, new AttributeValue
                        {
                            S = airlineInfo.Name
                        }
                    }
                },
            };

            await _connection.PutItemAsync(request);

            return new OkObjectResult(new ResponseObject("Airline created successfully"));
        }

        public async Task<IActionResult> CreateAirport(AirportModel airportInfo)
        {

            await CheckIfAirportExists(airportInfo.Name);

            var request = new PutItemRequest
            {
                TableName = DbConstants.TableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    {DbConstants.PK, new AttributeValue
                        {
                            S = DbConstants.AirportPrefix + Guid.NewGuid()
                        }
                    },
                    {DbConstants.SK, new AttributeValue
                        {
                            S = airportInfo.Name
                        }
                    }
                },
                Expected = new Dictionary<string, ExpectedAttributeValue>()
                {
                    {DbConstants.SK, new ExpectedAttributeValue(false)}
                },
            };

            await _connection.PutItemAsync(request);

            return new OkObjectResult(new ResponseObject("Airport created successfully"));
        }

        public async Task<IActionResult> CreateSection(SectionBindingModel sectionInfo)
        {
            var seatClass = sectionInfo.SeatClass.ToLower() == "first" ? SeatClass.First : sectionInfo.SeatClass.ToLower() == "bussiness" ? SeatClass.Bussiness : SeatClass.Economy;
            var flightNumber = DbConstants.FlightPrefix + sectionInfo.FlightNumber;

            await CheckIfFlightExists(flightNumber);

            await CheckIfFlightHasSeatClass(seatClass, flightNumber);

            var sectionId = DbConstants.SectionPrefix + Guid.NewGuid();

            var request = new PutItemRequest()
            {
                TableName = DbConstants.TableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    {DbConstants.PK, new AttributeValue() {S = sectionId } },
                    {DbConstants.SK, new AttributeValue() {S = flightNumber} },
                    {DbConstants.Data, new AttributeValue() {M = new Dictionary<string, AttributeValue>
                    {
                        {"Rows" , new AttributeValue {N = sectionInfo.Rows.ToString() } },
                        {"Cols" , new AttributeValue {N = sectionInfo.Columns.ToString() } },
                        {"AvailableSeats" , new AttributeValue {N = (sectionInfo.Rows*sectionInfo.Columns).ToString() } },
                        {"SeatClass" , new AttributeValue {S = seatClass.ToString() } },
                    }}},
                }
            };

            await _connection.PutItemAsync(request);

            
            var putRequests = new List<PutRequest>();

            for (int row = 0; row < sectionInfo.Rows; row++)
            {
                for (int col = 0; col < sectionInfo.Columns; col++)
                {
                    putRequests.Add(new PutRequest()
                    {
                        Item = new Dictionary<string, AttributeValue>
                       {
                           {DbConstants.PK , new AttributeValue {S = DbConstants.SeatPrefix + Guid.NewGuid() } },
                           {DbConstants.SK , new AttributeValue {S = sectionId } },
                           {DbConstants.Data , new AttributeValue {M = new Dictionary<string, AttributeValue> 
                           {
                               {"Row" , new AttributeValue {N = (row+1).ToString()} },
                               {"Column" , new AttributeValue {N = (col+1).ToString()} },
                               {"IsBooked" , new AttributeValue {BOOL = false} },
                           }}},
                       }
                    });
                }
            }

            int length = putRequests.Count;
            int index = 0;

            for (int i = 1; i <= Math.Ceiling(length/25f); i++)
            {
                var requestItems = new Dictionary<string, List<WriteRequest>>();
                requestItems.Add(DbConstants.TableName, new List<WriteRequest>());

                for (int j = 0; j < (length > 25 ? 25 : length); j++)
                {
                    requestItems[DbConstants.TableName].Add(new WriteRequest(putRequests[j + index]));
                }


                index += 25;
                length -= 25;

                var batchRequest = new BatchWriteItemRequest()
                {
                    RequestItems = requestItems
                };

                await _connection.BatchWriteItemAsync(batchRequest);
            }

            return new OkObjectResult(new ResponseObject("Section created"));
        }

        private async Task CheckIfAirlineExists(string name)
        {
            var request = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"{DbConstants.SK} = :airlineName and begins_with({DbConstants.PK} , :airlinePrefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":airlineName" , new AttributeValue() {S = name} },
                    {":airlinePrefix", new AttributeValue() {S = DbConstants.AirlinePrefix } }
                }
            };

            var items = await _connection.ScanAsync(request);

            if (items.Count > 0)
                throw new ArgumentException(ErrorMessages.AirlineExists);
        }

        private async Task CheckIfAirportExists(string name)
        {
            var request = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"{DbConstants.SK} = :name and begins_with({DbConstants.PK} , :prefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":name" , new AttributeValue() {S = name} },
                    {":prefix", new AttributeValue() {S = DbConstants.AirportPrefix } }
                }
            };

            var items = await _connection.ScanAsync(request);

            if (items.Count > 0)
                throw new ArgumentException(ErrorMessages.AirportExists);
        }

        private async Task CheckIfFlightExists(string flightNumber)
        {
            var request = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"{DbConstants.PK} = :flightNumber",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":flightNumber" , new AttributeValue() {S = flightNumber} },
                }
            };

            var items = await _connection.ScanAsync(request);

            if (items.Count == 0)
                throw new ArgumentException(ErrorMessages.FlightNotFound);
        }

        private async Task CheckIfFlightHasSeatClass(SeatClass seatClass, string flightNumber)
        {
            var request = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"{DbConstants.SK} = :flightNumber",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":flightNumber" , new AttributeValue {S = flightNumber } },
                }
            };

            var items = (await _connection.ScanAsync(request)).Items;

            foreach (var section in items)
            {
                section.TryGetValue(DbConstants.Data, out var data);
                data.M.TryGetValue("SeatClass", out var sectionSeatClass);

                if (sectionSeatClass.S == seatClass.ToString())
                {
                    throw new ArgumentException(ErrorMessages.SeatClassExists);
                }
            }
        }
    }
}
