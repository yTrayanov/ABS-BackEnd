using AirlineBookingSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineBookingSystem.Data.Configurations
{
    public class SeatConfiguration : IEntityTypeConfiguration<Seat>
    {
        public void Configure(EntityTypeBuilder<Seat> builder)
        {

            builder
                .HasOne(s => s.Section)
                .WithMany(s => s.Seats)
                .HasForeignKey(s => s.SectionId);

            builder
                .HasOne(s => s.Ticket)
                .WithOne(t => t.Seat)
                .HasForeignKey<Ticket>(t => t.SeatId);
        }
    }
}
