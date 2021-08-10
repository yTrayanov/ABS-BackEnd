using AirlineBookingSystem.Data.Configurations;
using AirlineBookingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AirlineBookingSystem.Data
{
    public class ABSContext : IdentityDbContext <User>
    {
        public ABSContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Airport> Airports { get; set; }

        public DbSet<Flight> Flights { get; set;}


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new FlightConfiguration());
            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new SeatConfiguration());
            builder.ApplyConfiguration(new TicketConfiguration());

            base.OnModelCreating(builder);
        }
    }
}
