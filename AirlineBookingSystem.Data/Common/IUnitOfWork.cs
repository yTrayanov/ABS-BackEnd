using AirlineBookingSystem.Models;
using System;
using System.Threading.Tasks;

namespace AirlineBookingSystem.Data.Common
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Airline> Airlines { get; }
        IGenericRepository<Airport> Airports { get; }
        IGenericRepository<Flight> Flights { get; }
        IGenericRepository<Seat> Seats { get; }
        IGenericRepository<Section> Sections { get; }
        IGenericRepository<Ticket> Tickets { get; }
        Task Save();

    }
}
