using AirlineBookingSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineBookingSystem.Data.Configurations
{
    public class FlightConfiguration : IEntityTypeConfiguration<Flight>
    {
        public void Configure(EntityTypeBuilder<Flight> builder)
        {

            builder
               .HasOne(f => f.Airline)
               .WithMany(a => a.Flights)
               .HasForeignKey(f => f.AirlineId);

            builder
                .HasMany(f => f.Sections)
                .WithOne(s => s.Flight)
                .HasForeignKey(s => s.FlightId);

            builder
                .HasOne(f => f.OriginAirport)
                .WithMany(a => a.DepartingFlights)
                .HasForeignKey(f => f.OriginAirportId);

            builder
                .HasOne(f => f.DestinationAirport)
                .WithMany(a => a.IncomingFlights)
                .HasForeignKey(f => f.DestinationAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(f => f.Tickets)
                .WithOne(t => t.Flight)
                .HasForeignKey(t => t.FlightId);

            builder
                .HasAlternateKey(f => f.FlightNumber);
        }
    }
}
