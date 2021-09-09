﻿using ABS_Flights.Models;
using AirlineBookingSystem.Models;
using ABS_Common.ResponsesModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirlineBookingSystem.Common;
using System.Data;
using AirlineBookingSystem.Common.Extensions;
using Dapper;

namespace ABS_Flights.Controllers
{
    [ApiController]
    [Route("flight")]
    public class FlightController : Controller
    {
        private IDbConnection _connection;

        public FlightController(ContextService contextService)
        {
            _connection = contextService.Connection;
        }

        [HttpGet("filter/{OriginAirport}/{DestinationAirport}/{DepartureDate}/{MembersCount}")]
        public async Task<IActionResult> FilterOneWayFlights([FromRoute] OneWayFlightModel flightInfo)
        {
            var flights = await FilterFlights(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            return new OkObjectResult(new ResponseObject("Flights found", flights.Select(f => new Flight[] { f })));
        }

        [HttpGet("filter/{OriginAirport}/{DestinationAirport}/{DepartureDate}/{MembersCount}/{ReturnDate}")]
        public async Task<IActionResult> FilterTwoWayFlights([FromRoute] TwoWaySearchModel flightInfo)
        {
            var toDestinationFlights = await FilterFlights(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            if (!toDestinationFlights.Any())
                return new OkObjectResult(new ResponseObject("There are no to destination flights"));

            var returnFlights = await FilterFlights(flightInfo.DestinationAirport, flightInfo.OriginAirport, flightInfo.ReturnDate, flightInfo.MembersCount);
            if (!returnFlights.Any())
                return new OkObjectResult(new ResponseObject("There are no return flights"));

            List<Flight[]> result = new List<Flight[]>();

            for (int i = 0; i < toDestinationFlights.Count; i++)
            {
                for (int j = 0; j < returnFlights.Count; j++)
                {
                    var flights = new Flight[] { toDestinationFlights[i], returnFlights[j] };
                    result.Add(flights);
                }
            }

            return new OkObjectResult(new ResponseObject("Flights found", result));
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateFlight([FromBody] FlightBindingModel flightInfo)
        {
            await _connection.QueryAsync<string>
                ($"EXEC usp_Flights_Insert '{flightInfo.OriginAirport}','{flightInfo.DestinationAirport}','{flightInfo.Airline}', '{flightInfo.FlightNumber}', '{flightInfo.DepartureDate}', '{flightInfo.LandingDate}'");

            return new OkObjectResult(new ResponseObject("Flight created"));
        }


        [HttpGet("{multipleIdsAsString}")]
        public async Task<IActionResult> GetFlightsByIds([FromRoute] string multipleIdsAsString)
        {

            List<int> ids = multipleIdsAsString.Split(',').Select(int.Parse).ToList();

            var data = new List<FlightIdModel>();
            foreach (var id in ids)
            {
                data.Add(new FlightIdModel { FlightId = id });
            }

            string query = $"EXEC usp_FlightsByMultipleIds_Select @FlightIds";

            using (var multi = await _connection.QueryMultipleAsync(query, new { FlightIds = data.ToDataTable().AsTableValuedParameter("FlightIdList") }))
            {

                var flights = (await multi.ReadAsync<Flight>()).ToList();

                if (flights.Count < ids.Count)
                {
                    return new BadRequestObjectResult(new ResponseObject("Could not find all selected flights"));
                }
                var sections = (await multi.ReadAsync<Section>()).ToList();
                var seats = (await multi.ReadAsync<Seat>()).ToList();

                foreach (var flight in flights)
                {
                    flight.Sections = sections.Where(section => section.FlightId == flight.Id).ToList();

                    foreach (var section in flight.Sections)
                    {
                        section.Seats = seats.Where(seat => seat.SectionId == section.Id).ToList();
                    }
                }


                return new OkObjectResult(new ResponseObject( "Flights found", flights));
            }

        }

        [HttpGet("information/all")]
        public async Task<IActionResult> GetAllFlights()
        {
            var flights = await _connection.QueryAsync<Flight>($"EXEC usp_AllFlights_Select");

            return new OkObjectResult(new ResponseObject("Flights for all flights", flights));

        }

        [HttpGet("information/{Id}")]
        public async Task<IActionResult> GetFlightInformation([FromRoute] string id)
        {

            using (var multi = await _connection.QueryMultipleAsync($"EXEC usp_FlightById_Select {id}"))
            {
                var flight = await multi.ReadSingleAsync<Flight>();
                flight.Sections = (await multi.ReadAsync<Section>()).ToList();

                var seats = (await multi.ReadAsync<Seat>()).ToList();
                var tickets = (await multi.ReadAsync<Ticket>()).ToList();

                foreach (var seat in seats)
                {
                    seat.Ticket = tickets.FirstOrDefault(ticket => ticket.SeatId == seat.Id);
                }

                foreach (var section in flight.Sections)
                {
                    section.Seats = seats.Where(seat => seat.SectionId == section.Id).ToList();
                }

                return new OkObjectResult(new ResponseObject("Flight information", flight));
            }
        }

        private async Task<IList<Flight>> FilterFlights(string originAirport, string destinationAirport, string departureDate, int membersCount)
        {
            var flights = (await _connection.QueryAsync<Flight>
                ($"EXEC dbo.usp_FilterFlights_Select '{originAirport}', '{destinationAirport}', '{departureDate}', {membersCount}")).ToList();

            return flights;
        }
    }
}
