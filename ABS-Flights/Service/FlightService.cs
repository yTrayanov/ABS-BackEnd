using ABS.Data.DynamoDbRepository;
using ABS_Common.ResponsesModels;
using ABS_Flights.Models;
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
        private IRepository<string, FlightModel> _flightRepository;

        public FlightService(IRepository<string , FlightModel> flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<IActionResult> CreateFlight(FlightModel flightInfo)
        {
            await _flightRepository.Add(flightInfo);

            return new OkObjectResult(new ResponseObject("Flight created"));
        }

        public async Task<IActionResult> FilterOneWayFlights(OneWayFlightModel flightInfo)
        {
            var flights = await FilterFlightsAsync(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            return new OkObjectResult(new ResponseObject("Flights found", flights.Select(f => new FlightModel[] { f })));
        }

        public async Task<IActionResult> FilterTwoWayFlights(TwoWaySearchModel flightInfo)
        {
            var toDestinationFlights = await FilterFlightsAsync(flightInfo.OriginAirport, flightInfo.DestinationAirport, flightInfo.DepartureDate, flightInfo.MembersCount);

            if (toDestinationFlights == null)
                return new OkObjectResult(new ResponseObject("There are no fligths to destination on this date"));

            var returnFlights = await FilterFlightsAsync(flightInfo.DestinationAirport, flightInfo.OriginAirport, flightInfo.ReturnDate, flightInfo.MembersCount);
            if (returnFlights == null)
                return new OkObjectResult(new ResponseObject("There are no return flights"));

            var result = new List<FlightModel[]>();

            for (int i = 0; i < toDestinationFlights.Count; i++)
            {
                for (int j = 0; j < returnFlights.Count; j++)
                {
                    var flights = new FlightModel[] { toDestinationFlights[i], returnFlights[j] };
                    result.Add(flights);
                }
            }

            return new OkObjectResult(new ResponseObject("Flights found", result));
        }

        public async Task<IActionResult> GetAllFlights()
        {
            var flights = await this._flightRepository.GetList("AllFlights");

            return new OkObjectResult(new ResponseObject("Flights for all flights", flights));
        }

        public async Task<IActionResult> GetFlightByIdAsync(string id)
        {
            var flight = await _flightRepository.Get(id);


            return new OkObjectResult(new ResponseObject("Flight information", flight));
        }

        public async Task<IActionResult> GetMultipleFlightsAsync(string flightIds)
        {

            var ids = flightIds.Split(',').ToList();

            string[] args = new string[] { "MultipleFlights" }.Concat(ids).ToArray();

            var flights = await _flightRepository.GetList(args);

            return new OkObjectResult(new ResponseObject("Flights found", flights.OrderBy(f => ids.IndexOf(f.Id))));
        }

        private async Task<IList<FlightModel>> FilterFlightsAsync(string originAirport, string destinationAirport, DateTime date, int membersCount)
        {
            var flights = await this._flightRepository.GetList(new string[] {"FilterFlights" , originAirport , destinationAirport , date.ToShortDateString(), membersCount.ToString()});

            return flights;
        }

    }
}
