using ABS_Flights.Models;
using AirlineBookingSystem.Models;
using ABS_Common.ResponsesModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirlineBookingSystem.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace ABS_Flights.Controllers
{
    [ApiController]
    [Route("flight")]
    public class FlightController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public FlightController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        [HttpGet("filter/{OriginAirport}/{DestinationAirport}/{DepartureDate}/{MembersCount}")]
        public async Task<IActionResult> FilterOneWayFlights([FromRoute] FlightBindingModel flightInfo)
        {
            var flights = await FilterFlights(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            return new OkObjectResult(new ResponseObject( "Flights found", flights.Select(f => new Flight[] { f })));
        }

        [HttpGet("filter/{OriginAirport}/{DestinationAirport}/{DepartureDate}/{MembersCount}/{ReturnDate}")]
        public async Task<IActionResult> FilterTwoWayFlights([FromRoute] FlightBindingModel flightInfo)
        {
            var toDestinationFlights = await FilterFlights(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            if(!toDestinationFlights.Any())
                return new OkObjectResult(new ResponseObject( "There are no to destination flights"));

            var returnFlights = await FilterFlights(flightInfo.DestinationAirport, flightInfo.OriginAirport, flightInfo.ReturnDate, flightInfo.MembersCount);
            if(returnFlights.Any())
                return new OkObjectResult(new ResponseObject( "There are no return flights"));

            List<Flight[]> result = new List<Flight[]>();

            for (int i = 0; i < toDestinationFlights.Count; i++)
            {
                for (int j = 0; j < returnFlights.Count; j++)
                {
                    var flights = new Flight[] { toDestinationFlights[i], returnFlights[j] };
                    result.Add(flights);
                }
            }

            return new OkObjectResult(new ResponseObject( "Flights found", result));
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateFlight([FromBody] FlightBindingModel flightInfo)
        {
                var searchFlight = await _unitOfWork.Flights.Get(f => f.FlightNumber == flightInfo.FlightNumber);

                if (searchFlight != null)
                    throw new ArgumentException("Flight number already exists");

                var originAirport = await _unitOfWork.Airports.Get(a => a.Name == flightInfo.OriginAirport);
                var destinationAirport = await _unitOfWork.Airports.Get(a => a.Name == flightInfo.DestinationAirport);

                if (originAirport == null || destinationAirport == null)
                    throw new ArgumentException("Invalid airports information");

                var airline = await _unitOfWork.Airlines.Get(a => a.Name == flightInfo.Airline);
                if (airline == null)
                    throw new ArgumentException("Airline does not exist");

                var flight = new Flight()
                {
                    OriginAirportId = originAirport.Id,
                    DestinationAirportId = destinationAirport.Id,
                    AirlineId = airline.Id,
                    FlightNumber = flightInfo.FlightNumber,
                    DepartureDate = flightInfo.DepartureDate,
                    LandingDate = flightInfo.LandingDate,
                };

                await this._unitOfWork.Flights.Insert(flight);
                await _unitOfWork.Save();

                return new OkObjectResult(new ResponseObject( "Flight created"));
        }


        [HttpGet("{multipleIdsAsString}")]
        public async  Task<IActionResult> GetFlightsByIds([FromRoute] string multipleIdsAsString)
        {

            string[] ids = multipleIdsAsString.Split(',');

            var flights = await _unitOfWork.Flights.GetAll(f => ids.Contains(f.Id.ToString()) , 
                include: f =>  f.Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Sections).ThenInclude(s => s.Seats));

            if (flights.Count < ids.Length)
            {
                return new BadRequestObjectResult(new ResponseObject( "Could not find all selected flights"));
            }

            return new OkObjectResult(new ResponseObject( "Flights found", flights));
        }

        [HttpGet("information/all")]
        public async Task<IActionResult> GetAllFlights()
        {
            var flights = await _unitOfWork.Flights
                      .GetAll(null,
                      include: f => f.Include(x => x.OriginAirport)
                                      .Include(x => x.DestinationAirport)
                                      .Include(x => x.Airline)
                                      .Include(x => x.Sections));

            return new OkObjectResult(new ResponseObject( "Flights for all flights", flights));
        }

        [HttpGet("information/{Id}")]
        public async Task<IActionResult> GetFlightInformation([FromRoute] string id)
        {
            var flight = await _unitOfWork.Flights.Get(f => f.Id.ToString() == id,
                include: f => f.Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Airline)
                .Include(f => f.Sections).ThenInclude(s => s.Seats).ThenInclude(s => s.Ticket).ThenInclude(t => t.User));

            flight.Sections = flight.Sections.Where(s => s.Seats.Any(s => s.IsBooked)).ToList();

            for (int i = 0; i < flight.Sections.Count; i++)
            {
                flight.Sections.ToList()[i].Seats = flight.Sections.ToList()[i].Seats.Where(s => s.IsBooked).ToList();
            }

            return new OkObjectResult(new ResponseObject( "Flight information", flight));
        }

        private async Task<IList<Flight>> FilterFlights(string originAirport, string destinationAirport, DateTime departureDate, int membersCount)
        {
            var flights = await _unitOfWork.Flights
                   .GetAll( f => f.OriginAirport.Name == originAirport && f.DestinationAirport.Name == destinationAirport  && f.DepartureDate.Date == departureDate.Date,
                   include: f => f.Include(x => x.OriginAirport)
                                   .Include(x => x.DestinationAirport)
                                   .Include(x => x.Airline)
                                   .Include(x => x.Sections));

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
