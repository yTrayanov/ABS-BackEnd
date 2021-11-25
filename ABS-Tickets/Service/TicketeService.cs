using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS.Data.DynamoDbRepository;
using ABS_Common.Enumerations;
using ABS_Common.ResponsesModels;
using ABS_Data.Data;
using ABS_Tickets.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Tickets.Service
{
    public class TicketeService : ITicketService
    {
        private IAmazonDynamoDB _connection;
        private IRepository<string, TicketModel> _ticketRepository;

        public TicketeService(ABSContext context , IRepository<string , TicketModel> ticketRepository)
        {
            _connection = context.CreateConnection();
            this._ticketRepository = ticketRepository;
        }

        public async Task<IActionResult> CreateTicket(TicketCreateModel model, string username)
        {
            var flightIds = model.FlightIds;
            var seats = model.Seats;

            var tickets = new List<TicketModel>();

            for (int flightIndex = 0; flightIndex < flightIds.Length; flightIndex++)
            {
                for (int seatIndex = 0; seatIndex < seats[flightIndex].Length; seatIndex++)
                {
                    var currentSeat = seats[flightIndex][seatIndex];
                    tickets.Add(new TicketModel()
                    {
                        FlightId = flightIds[flightIndex],
                        SeatId = currentSeat.Id,
                        Username = username,
                        PassengerName = currentSeat.PassengerName,
                    });
                }
            }

            await _ticketRepository.AddRange(tickets);

            return new OkObjectResult(new ResponseObject("Seats booked successfully"));
        }

        public async Task<IActionResult> GetUserTickets(string username)
        {
            var tickets = await _ticketRepository.GetList(username);
            return new OkObjectResult(new ResponseObject("User tickets here", tickets));
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
    }

}
