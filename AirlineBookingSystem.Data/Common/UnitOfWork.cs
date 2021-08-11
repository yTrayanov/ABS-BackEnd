using AirlineBookingSystem.Models;
using System;
using System.Threading.Tasks;

namespace AirlineBookingSystem.Data.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ABSContext _context;
        private IGenericRepository<Airline> _arilines;
        private IGenericRepository<Airport> _airports;
        private IGenericRepository<Flight> _flights;
        private IGenericRepository<Seat> _seats;
        private IGenericRepository<Section> _sections;
        private IGenericRepository<Ticket> _tickets;

        public UnitOfWork(ABSContext context)
        {
            this._context = context;
        }

        public IGenericRepository<Airline> Airlines => _arilines ??= new GenericRepository<Airline>(_context);

        public IGenericRepository<Airport> Airports => _airports ??= new GenericRepository<Airport>(_context);

        public IGenericRepository<Flight> Flights => _flights ??= new GenericRepository<Flight>(_context);

        public IGenericRepository<Seat> Seats => _seats ??= new GenericRepository<Seat>(_context);

        public IGenericRepository<Section> Sections => _sections ??= new GenericRepository<Section>(_context);

        public IGenericRepository<Ticket> Tickets => _tickets ??= new GenericRepository<Ticket>(_context);


        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
