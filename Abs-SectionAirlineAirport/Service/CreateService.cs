using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
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
                    {AirlineDbModel.Id, new AttributeValue
                        {
                            S = AirlineDbModel.Prefix + Guid.NewGuid()
                        }
                    },

                    {AirlineDbModel.Name, new AttributeValue
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
                    {AirportDbModel.Id, new AttributeValue
                        {
                            S = AirportDbModel.Prefix + Guid.NewGuid()
                        }
                    },
                    {AirportDbModel.Name, new AttributeValue
                        {
                            S = airportInfo.Name
                        }
                    }
                }
            };

            await _connection.PutItemAsync(request);

            return new OkObjectResult(new ResponseObject("Airport created successfully"));
        }

        public async Task<IActionResult> CreateSection(SectionBindingModel sectionInfo)
        {
            var seatClass = sectionInfo.SeatClass.ToLower() == "first" ? SeatClass.First : sectionInfo.SeatClass.ToLower() == "bussiness" ? SeatClass.Bussiness : SeatClass.Economy;
            var flightNumber = FlightDbModel.Prefix + sectionInfo.FlightNumber;

            await CheckIfFlightExists(flightNumber);

            await CheckIfFlightHasSeatClass(seatClass, flightNumber);

            var sectionId = SectionDbModel.Prefix + Guid.NewGuid();

            var request = new PutItemRequest()
            {
                TableName = DbConstants.TableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    {SectionDbModel.Id, new AttributeValue() {S = sectionId } },
                    {SectionDbModel.FlightId, new AttributeValue() {S = flightNumber} },
                    {SectionDbModel.Data, new AttributeValue() {M = new Dictionary<string, AttributeValue>
                    {
                        {"Rows" , new AttributeValue {N = sectionInfo.Rows.ToString() } },
                        {"Columns" , new AttributeValue {N = sectionInfo.Columns.ToString() } },
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
                           {SeatDbModel.Id , new AttributeValue {S = SeatDbModel.Prefix+ Guid.NewGuid() } },
                           {SeatDbModel.SectionId , new AttributeValue {S = sectionId } },
                            {SeatDbModel.FlightId, new AttributeValue {S = flightNumber } },
                           {SeatDbModel.Data , new AttributeValue {M = new Dictionary<string, AttributeValue> 
                           {
                               {"Row" , new AttributeValue {N = (row+1).ToString()} },
                               {"Column" , new AttributeValue {N = (col+1).ToString()} },
                               {"IsBooked" , new AttributeValue {BOOL = false} },
                               {"SeatClass" , new AttributeValue {S = seatClass.ToString()}}
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
                FilterExpression = $"{AirlineDbModel.Name} = :airlineName and begins_with({AirlineDbModel.Id} , :airlinePrefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":airlineName" , new AttributeValue() {S = name} },
                    {":airlinePrefix", new AttributeValue() {S = AirlineDbModel.Prefix } }
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
                FilterExpression = $"{AirportDbModel.Name} = :name and begins_with({AirportDbModel.Id} , :prefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":name" , new AttributeValue() {S = name} },
                    {":prefix", new AttributeValue() {S = AirportDbModel.Prefix } }
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
                FilterExpression = $"{FlightDbModel.Id} = :flightNumber",
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
                FilterExpression = $"{FlightDbModel.Id} = :flightNumber",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":flightNumber" , new AttributeValue {S = flightNumber } },
                }
            };

            var items = (await _connection.ScanAsync(request)).Items;

            foreach (var section in items)
            {
                section.TryGetValue(FlightDbModel.Data, out var data);
                data.M.TryGetValue("SeatClass", out var sectionSeatClass);

                if (sectionSeatClass?.S == seatClass.ToString())
                {
                    throw new ArgumentException(ErrorMessages.SeatClassExists);
                }
            }
        }
    }
}
