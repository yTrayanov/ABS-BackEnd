using ABS_Common.ResponsesModels;
using ABS_Data.Data;
using ABS_Flights.Models;
using AirlineBookingSystem.Common.Extensions;
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

        private IDbConnection _connection;

        public FlightService(ContextService contextService)
        {
            _connection = contextService.Connection;
        }

        public async Task<IActionResult> CreateFlight(FlightBindingModel flightInfo)
        {
            string query = $"EXEC usp_Flights_Insert @OriginAirport, @DestinationAirport, @Airline, @FlightNumber , @DepartureDate, @LandingDate";

            var parameters = new { OriginAirport = flightInfo.OriginAirport, DestinationAirport = flightInfo.DestinationAirport, Airline = flightInfo.Airline, FlightNumber = flightInfo.FlightNumber, DepartureDate = flightInfo.DepartureDate, LandingDate = flightInfo.LandingDate };

            await _connection.QueryAsync<string>(query, parameters);

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
            var flights = await _connection.QueryAsync<AllFlightModel>($"EXEC usp_AllFlights_Select");

            return new OkObjectResult(new ResponseObject("Flights for all flights", flights));
        }

        public async Task<IActionResult> GetFlightById(string id)
        {
            using (var multi = await _connection.QueryMultipleAsync($"EXEC usp_FlightById_Select {id}"))
            {
                var flight = await multi.ReadSingleAsync<FlightInformation>();

                flight.Sections =
                    (await multi.ReadAsync<Section>())
                    .OrderBy(s => s.SeatClass)
                    .ToList();

                var seats = (await multi.ReadAsync<Seat>()).ToList();

                foreach (var section in flight.Sections)
                {
                    section.Seats = seats.Where(seat => seat.SectionId == section.Id).ToList();
                }

                return new OkObjectResult(new ResponseObject("Flight information", flight));
            }
        }

        public async Task<IActionResult> GetMultipleFlights(string flightIds)
        {

            List<int> ids = flightIds.Split(',').Select(int.Parse).ToList();

            var data = new List<FlightIdModel>();
            foreach (var id in ids)
            {
                data.Add(new FlightIdModel { FlightId = id });
            }

            string query = $"EXEC usp_FlightsByMultipleIds_Select @FlightIds";

            using (var multi = await _connection.QueryMultipleAsync(query, new { FlightIds = data.ToDataTable().AsTableValuedParameter("FlightIdList") }))
            {

                var flights = (await multi.ReadAsync<FlightWithSectionsModel>()).OrderBy(f => ids.IndexOf(f.Id)).ToList();

                if (flights.Count < ids.Count)
                {
                    return new BadRequestObjectResult(new ResponseObject("Could not find all selected flights"));
                }
                var sections = (await multi.ReadAsync<Section>()).ToList();
                var seats = (await multi.ReadAsync<Seat>()).ToList();

                foreach (var flight in flights)
                {
                    flight.Sections = sections.Where(section => section.FlightId == flight.Id).OrderBy(s => s.SeatClass).ToList();

                    foreach (var section in flight.Sections)
                    {
                        section.Seats = seats.Where(seat => seat.SectionId == section.Id).ToList();
                    }
                }


                return new OkObjectResult(new ResponseObject("Flights found", flights));
            }
        }

        private async Task<IList<FilteredFlightModel>> FilterFlights(string originAirport, string destinationAirport, DateTime departureDate, int membersCount)
        {
            string query = "EXEC dbo.usp_FilterFlights_Select @OriginAirport, @DestinationAirport, @DepartureDate, @MembersCount";

            var flights = (await _connection.QueryAsync<FilteredFlightModel>
                (query, new { OriginAirport = originAirport, DestinationAirport = destinationAirport, DepartureDate = departureDate, MembersCount = membersCount })).ToList();

            return flights;
        }
    }
}
