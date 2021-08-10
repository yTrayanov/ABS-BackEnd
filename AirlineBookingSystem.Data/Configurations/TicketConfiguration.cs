using AirlineBookingSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineBookingSystem.Data.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder
                .HasOne(t => t.Seat)
                .WithOne(s => s.Ticket)
                .HasForeignKey<Ticket>(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
