using ABS_Flights.Models;
using AirlineBookingSystem.Data.Interfaces;
using AirlineBookingSystem.Models;
using Common.ResponsesModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ABS_Flights.Controllers
{
    [ApiController]
    [Route("flight")]
    public class FlightController : Controller
    {
        private IFlightDbService _flightService;

        public FlightController(IFlightDbService service)
        {
            this._flightService = service;
        }

        [HttpGet("filter/{OriginAirport}/{DestinationAirport}/{DepartureDate}/{MembersCount}")]
        public IActionResult FilterOneWayFlights([FromRoute] FlightBindingModel flightInfo)
        {
            var flights = FilterFlights(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);


            if (!flights.Any())
            {
                return new OkObjectResult(new ResponseObject(true, "No flights found"));
            }


            return new OkObjectResult(new ResponseObject(true, "Flights found", flights.Select(f => new Flight[] { f })));
        }

        [HttpGet("filter/{OriginAirport}/{DestinationAirport}/{DepartureDate}/{MembersCount}/{ReturnDate}")]
        public IActionResult FilterTwoWayFlights([FromRoute] FlightBindingModel flightInfo)
        {
            var toDestinationFlights = FilterFlights(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            if (!toDestinationFlights.Any())
            {
                return new OkObjectResult(new ResponseObject(true, "There are no fligths to destination on this date"));
            }

            var returnFlights = FilterFlights(flightInfo.DestinationAirport, flightInfo.OriginAirport, flightInfo.ReturnDate, flightInfo.MembersCount);

            if (!returnFlights.Any())
            {
                return new OkObjectResult(new ResponseObject(true, "There are no return flights on this date"));
            }

            List<Flight[]> result = new List<Flight[]>();

            for (int i = 0; i < toDestinationFlights.Count; i++)
            {
                for (int j = 0; j < returnFlights.Count; j++)
                {
                    var flights = new Flight[] { toDestinationFlights[i], returnFlights[j] };
                    result.Add(flights);
                }
            }



            return new OkObjectResult(new ResponseObject(true, "Flights found", result));
        }


        [HttpPost("create")]
        public IActionResult CreateFlight([FromBody] FlightBindingModel flightInfo)
        {
            try
            {
                this._flightService.CreateFlight(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.Airline, flightInfo.FlightNumber, flightInfo.DepartureDate, flightInfo.LandingDate);

                return new OkObjectResult(new ResponseObject(true, "Flight created"));
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseObject(false, "Could not create flight", e.Message));
            }
        }


        [HttpGet("{multipleIdsAsString}")]
        public IActionResult GetFlightsByIds([FromRoute] string multipleIdsAsString)
        {
            string[] ids = multipleIdsAsString.Split(',');

            var flights = this._flightService.GetFlightsByIds(ids);

            if (flights.Count < ids.Length)
            {
                return new BadRequestObjectResult(new ResponseObject(false, "Could not find all selected flights"));
            }

            return new OkObjectResult(new ResponseObject(true, "Flights found", flights));
        }

        [HttpGet("information/all")]
        public IActionResult GetAllFlights()
        {
            var flights = this._flightService.GetFlights();

            return new OkObjectResult(new ResponseObject(true, "Flights for all flights", flights));
        }

        [HttpGet("information/{Id}")]
        public IActionResult GetFlightInformation([FromRoute] string id)
        {
            var flight = this._flightService.GetFlightInformation(id);
            flight.Sections = flight.Sections.Where(s => s.Seats.Any(s => s.IsBooked)).ToList();

            for (int i = 0; i < flight.Sections.Count; i++)
            {
                flight.Sections.ToList()[i].Seats = flight.Sections.ToList()[i].Seats.Where(s => s.IsBooked).ToList();
            }

            return new OkObjectResult(new ResponseObject(true, "Flight information", flight));
        }

        private List<Flight> FilterFlights(string originAirport, string destinationAirport, DateTime departureDate, int membersCount)
        {
            var flights = this._flightService.GetFlights().Where(f => f.OriginAirport.Name == originAirport && f.DestinationAirport.Name == destinationAirport && f.DepartureDate.Date == departureDate.Date).ToList();

            for (int i = 0; i < flights.Count; i++)
            {
                if (flights[i].Sections.Any(s => s.AvailableSeatsCount >= membersCount))
                {
                    continue;
                }
                flights.RemoveAt(i);
                i--;
            }

            return flights;
        }

    }
}
