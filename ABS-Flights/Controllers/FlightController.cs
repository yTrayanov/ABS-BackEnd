using ABS_Flights.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ABS_Flights.Service;

namespace ABS_Flights.Controllers
{
    [ApiController]
    [Route("flight")]
    public class FlightController : Controller
    {
        private IFlightService _flightService;

        public FlightController(IFlightService flightService)
        {
            this._flightService = flightService;
        }

        [HttpGet("filter/{OriginAirport}/{DestinationAirport}/{DepartureDate}/{MembersCount}")]
        public async Task<IActionResult> FilterOneWayFlights([FromRoute] OneWayFlightModel flightInfo)
        {
            return await _flightService.FilterOneWayFlights(flightInfo);
        }

        [HttpGet("filter/{OriginAirport}/{DestinationAirport}/{DepartureDate}/{MembersCount}/{ReturnDate}")]
        public async Task<IActionResult> FilterTwoWayFlights([FromRoute] TwoWaySearchModel flightInfo)
        {
            return await _flightService.FilterTwoWayFlights(flightInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateFlight([FromBody] FlightBindingModel flightInfo)
        {
            return await this._flightService.CreateFlight(flightInfo);
        }

        [HttpGet("{multipleIdsAsString}")]
        public async Task<IActionResult> GetFlightsByIds([FromRoute] string multipleIdsAsString)
        {
            return await _flightService.GetMultipleFlights(multipleIdsAsString);
        }

        [HttpGet("information/all")]
        public async Task<IActionResult> GetAllFlights()
        {
            return await _flightService.GetAllFlights();
        }

        [HttpGet("information/{id}")]
        public async Task<IActionResult> GetFlightInformation([FromRoute] string id)
        {
            return await _flightService.GetFlightById(id);
        }
    }
}
