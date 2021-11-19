using Abs.Common.Constants;
using Abs.Common.Constants.DbModels;
using ABS_Common.Enumerations;
using ABS_Common.ResponsesModels;
using ABS_Data.Data;
using ABS_Tickets.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABS_Tickets.Service
{
    public class TicketeService : ITicketService
    {
        private IAmazonDynamoDB _connection;

        public TicketeService(ABSContext context)
        {
            _connection = context.CreateConnection();
        }
        public async Task<IActionResult> CreateTicket(TicketCreateModel model, string username)
        {
            var flightIds = model.FlightIds;
            var seats = model.Seats;


            var seatIds = new List<string>();

            for (int row = 0; row < seats.Length; row++)
                for (int col = 0; col < seats[row].Length; col++)
                    seatIds.Add(seats[row][col].Id);

            var transactItems = new List<TransactWriteItem>();

            foreach (var seatId in seatIds)
            {
                transactItems.Add(new TransactWriteItem
                {
                    Update = new Update
                    {
                        TableName = DbConstants.TableName,
                        Key = new Dictionary<string, AttributeValue>
                        {
                            {SeatDbModel.Id , new AttributeValue {S = seatId } },
                            {SeatDbModel.FlightId, new AttributeValue {S = seats[0].Select(s=>s.Id).Contains(seatId) ? FlightDbModel.Prefix + flightIds[0] :FlightDbModel.Prefix + flightIds[1]} }
                        },
                        UpdateExpression = $"SET #data.IsBooked = :isBooked",
                        ExpressionAttributeNames = new Dictionary<string, string>
                        {
                            {"#data" , SeatDbModel.Data }
                        },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            {":isBooked", new AttributeValue {BOOL = true} },
                        },
                    }
                });
            }


            var bookRequest = new TransactWriteItemsRequest()
            {
                TransactItems = transactItems,
            };

            var response = await _connection.TransactWriteItemsAsync(bookRequest);

            var putRequests = new List<PutRequest>();

            for (int flightIndex = 0; flightIndex < flightIds.Length; flightIndex++)
            {
                for (int seatIndex = 0; seatIndex < seats[flightIndex].Length; seatIndex++)
                {
                    putRequests.Add(new PutRequest()
                    {
                        Item = new Dictionary<string, AttributeValue>
                        {
                            {TicketDbModel.Id , new AttributeValue {S = TicketDbModel.Prefix + Guid.NewGuid() } },
                            {TicketDbModel.UserId , new AttributeValue {S = username } },
                            {TicketDbModel.FlightId, new AttributeValue {S = FlightDbModel.Prefix + flightIds[flightIndex].ToString() } },
                            {TicketDbModel.SeatId, new AttributeValue {S = seats[flightIndex][seatIndex].Id} },
                            {TicketDbModel.Data, new AttributeValue {M = new Dictionary<string, AttributeValue>
                            {
                                {"PassengerName", new AttributeValue {S = seats[flightIndex][seatIndex].PassengerName } }
                            }
                            }}
                        }
                    });
                }
            }

            int length = putRequests.Count;
            int index = 0;

            for (int i = 1; i <= Math.Ceiling(length / 25f); i++)
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

            return new OkObjectResult(new ResponseObject("Seats booked successfully"));
        }

        public async Task<IActionResult> GetUserTickets(string username)
        {

            var requestTickets = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"begins_with({TicketDbModel.Id},:ticketPrefix) AND {TicketDbModel.UserId} = :userId ",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":ticketPrefix", new AttributeValue {S = TicketDbModel.Prefix } },
                    {":userId", new AttributeValue {S = username} },
                }
            };

            var ticketResponseItems = (await _connection.ScanAsync(requestTickets)).Items;

            if (ticketResponseItems.Count == 0)
                return new OkObjectResult(new ResponseObject("User has no tickets"));

            var tickets = new List<TicketViewModel>();
            var ids = new List<string>();

            foreach (var item in ticketResponseItems)
            {
                item.TryGetValue(TicketDbModel.Id, out var ticketId);
                item.TryGetValue(TicketDbModel.FlightId, out var flightId);
                item.TryGetValue(TicketDbModel.Data, out var data);
                item.TryGetValue(TicketDbModel.SeatId, out var seatId);

                ids.Add(flightId.S);
                ids.Add(seatId.S);

                data.M.TryGetValue("PassengerName", out var passangerName);

                tickets.Add(new TicketViewModel
                {
                    FlightId = flightId.S,
                    Id = ticketId.S,
                    PassengerName = passangerName.S,
                    SeatId = seatId.S
                });
            }

            var mapAttValues = new Dictionary<string, AttributeValue>();

            int index = 0;
            foreach (var id in ids)
            {
                index++;
                mapAttValues.Add($":flightId{index}", new AttributeValue { S = ids[index - 1] });
            }

            string stringIdentifiers = string.Join(",", mapAttValues.Keys);

            var request = new ScanRequest()
            {
                TableName = DbConstants.TableName,
                FilterExpression = $"(begins_with({FlightDbModel.Id} , :flightPrefix) AND {FlightDbModel.Id} IN ({stringIdentifiers}))" +
                                $"OR(begins_with({SeatDbModel.Id}, :seatPrefix) AND {SeatDbModel.Id} IN ({stringIdentifiers}))",

                ExpressionAttributeValues = new Dictionary<string, AttributeValue>(mapAttValues)
                {
                    {":flightPrefix" , new AttributeValue {S = FlightDbModel.Prefix } },
                    {":seatPrefix" , new AttributeValue {S = SeatDbModel.Prefix } },
                }
            };

            var responseItems = (await _connection.ScanAsync(request)).Items;

            foreach (var item in responseItems.Where(item =>
            {
                item.TryGetValue(DbConstants.PK, out var id);
                return (id.S.StartsWith(FlightDbModel.Prefix));
            }))
            {
                item.TryGetValue(FlightDbModel.Id, out var flightId);
                item.TryGetValue(FlightDbModel.Id, out var id);
                item.TryGetValue(FlightDbModel.AirlineId, out var airlineId);
                item.TryGetValue(FlightDbModel.Data, out var data);
                item.TryGetValue(FlightDbModel.OriginAirportId, out var originAirportId);
                item.TryGetValue(FlightDbModel.DestinationAirportId, out var destinationAirportId);

                data.M.TryGetValue("DepartureDate", out var departureDate);
                data.M.TryGetValue("LandingDate", out var landingDate);

                var flight = new Flight()
                {
                    Id = flightId.S,
                    Airline = await GetAirlineName(airlineId.S),
                    OriginAirport = await GetAirportName(originAirportId.S),
                    DestinationAirport = await GetAirportName(originAirportId.S),
                    FlightNumber = flightId.S.Replace(FlightDbModel.Prefix , ""),
                    DepartureDate = DateTime.Parse(departureDate.S),
                    LandingDate = DateTime.Parse(landingDate.S),
                };

                var flightTickets = tickets.Where(t => t.FlightId == flightId.S).ToList();

                foreach (var ticket in flightTickets)
                {
                    ticket.Flight = flight;
                }
            }

            foreach (var item in responseItems.Where(item =>
            {
                item.TryGetValue(DbConstants.PK, out var id);
                return (id.S.StartsWith(SeatDbModel.Prefix));
            }))
            {

                item.TryGetValue(SeatDbModel.Id, out var seatId);
                item.TryGetValue(SeatDbModel.FlightId, out var flightId);
                item.TryGetValue(SeatDbModel.Data, out var data);

                data.M.TryGetValue("Row", out var row);
                data.M.TryGetValue("Column", out var column);
                data.M.TryGetValue("SeatClass", out var seatClass);
                data.M.TryGetValue("IsBooked", out var isBooked);

                var seat = new SeatModel()
                {
                    Id = seatId.S,
                    FlightId = flightId.S,
                    Row = int.Parse(row.N),
                    Column = int.Parse(column.N),
                    SeatClass = seatClass.S == SeatClass.First.ToString() ? SeatClass.First : seatClass.S == SeatClass.Bussiness.ToString() ? SeatClass.Bussiness : SeatClass.Economy,
                    IsBooked = isBooked.BOOL,
                };

                tickets.FirstOrDefault(t => t.SeatId == seat.Id).Seat = seat;
            }


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
