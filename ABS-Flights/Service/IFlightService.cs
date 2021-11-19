using ABS_Flights.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABS_Flights.Service
{
    public interface IFlightService
    {
        Task<IActionResult> FilterOneWayFlights(OneWayFlightModel flightInfo);
        Task<IActionResult> FilterTwoWayFlights(TwoWaySearchModel flightInfo);
        Task<IActionResult> CreateFlight(FlightModel flightInfo);
        Task<IActionResult> GetMultipleFlightsAsync(string ids);
        Task<IActionResult> GetAllFlights();
        Task<IActionResult> GetFlightByIdAsync(string id);

    }
}
