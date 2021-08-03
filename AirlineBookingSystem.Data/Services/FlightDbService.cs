using AirlineBookingSystem.Data.Interfaces;
using AirlineBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AirlineBookingSystem.Data.Services
{
    public class FlightDbService : BaseService , IFlightDbService
    {
        public FlightDbService(ABSContext context) : base(context)
        {
        }

        public void CreateFlight(string originAirportName, string destinationAirportName, string airlineName, string flightNumber, DateTime departureDate, DateTime landingDate)
        {
            if (this.Context.Flights.Any(f => f.FlightNumber == flightNumber))
                throw new ArgumentException("Flight number already exists");

            var originAirport = this.Context.Airports.FirstOrDefault(a => a.Name == originAirportName);
            var destinationAirport = this.Context.Airports.FirstOrDefault(a => a.Name == destinationAirportName);

            if (originAirport == null || destinationAirport == null)
                throw new ArgumentException("Invalid airport information");

            var airline = this.Context.Airlines.FirstOrDefault(a => a.Name == airlineName);
            if (airline == null)
                throw new ArgumentException("Airline does not exist");

            var flight = new Flight()
            {
                OriginAirport = originAirport,
                DestinationAirport = destinationAirport,
                Airline = airline,
                FlightNumber = flightNumber,
                DepartureDate =departureDate,
                LandingDate = landingDate,
            };

            this.Context.Flights.Add(flight);
            this.Context.SaveChanges();
        }

        public Flight GetFlightInformation(string id)
        {
            return this.Context.Flights
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Airline)
                .Include(f => f.Sections).ThenInclude(s => s.Seats).ThenInclude(s => s.Ticket).ThenInclude(t => t.User)
                .FirstOrDefault(f => f.Id == int.Parse(id));

        }

        public ICollection<Flight> GetFlights()
        {
            return this.Context.Flights
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Airline)
                .Include(f => f.Sections)
                .ToList();
        }

        public ICollection<Flight> GetFlightsByIds(string[] ids)
        {
            return this.Context.Flights
                .Where(f => ids.Contains(f.Id.ToString()))
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Sections)
                    .ThenInclude(s => s.Seats)
                .ToList();
        }
    }
}
