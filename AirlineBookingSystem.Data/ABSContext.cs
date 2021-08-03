using AirlineBookingSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace AirlineBookingSystem.Data
{
    public class ABSContext : DbContext 
    {
        public ABSContext(DbContextOptions<ABSContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Airport> Airports { get; set; }

        public DbSet<Flight> Flights { get; set;}

        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
         

        protected override void OnModelCreating(ModelBuilder builder)
        {
            #region User & Role

            builder.Entity<User>()
                .HasMany(u => u.Tickets)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            builder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId);

            builder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId);

            #endregion


            #region Flight
            builder.Entity<Flight>()
                .HasOne(f => f.Airline)
                .WithMany(a => a.Flights)
                .HasForeignKey(f => f.AirlineId);

            builder.Entity<Flight>()
                .HasMany(f => f.Sections)
                .WithOne(s => s.Flight)
                .HasForeignKey(s => s.FlightId);

            builder.Entity<Flight>()
                .HasOne(f => f.OriginAirport)
                .WithMany(a => a.DepartingFlights)
                .HasForeignKey(f => f.OriginAirportId);

            builder.Entity<Flight>()
                .HasOne(f => f.DestinationAirport)
                .WithMany(a => a.IncomingFlights)
                .HasForeignKey(f => f.DestinationAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Flight>()
                .HasMany(f => f.Tickets)
                .WithOne(t => t.Flight)
                .HasForeignKey(t => t.FlightId);

            builder.Entity<Flight>()
                .HasAlternateKey(f => f.FlightNumber);
            #endregion

            #region Seat

            builder.Entity<Seat>()
                .HasOne(s => s.Section)
                .WithMany(s => s.Seats)
                .HasForeignKey(s => s.SectionId);

            builder.Entity<Seat>()
                .HasOne(s => s.Ticket)
                .WithOne(t => t.Seat)
                .HasForeignKey<Ticket>(t => t.SeatId);


            #endregion

            builder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithOne(s => s.Ticket)
                .HasForeignKey<Ticket>(t => t.SeatId);

            base.OnModelCreating(builder);
        }
    }
}
