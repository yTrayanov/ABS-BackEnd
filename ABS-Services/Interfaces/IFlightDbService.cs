using AirlineBookingSystem.Models;
using System;
using System.Collections.Generic;

namespace ABS_Services.Interfaces
{
    public interface IFlightDbService
    {
        ICollection<Flight> GetFlights();

        ICollection<Flight> GetFlightsByIds(string[] ids);
        void CreateFlight(string originAirportName, string destinationAirportName, string airlineName, string flightNumber, DateTime departureDate, DateTime returnDate);

        Flight GetFlightInformation(string id);
    }
}
