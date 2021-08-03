using AirlineBookingSystem.Data.Interfaces;
using AirlineBookingSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AirlineBookingSystem.Data.Services
{
    public class TicketDbService : BaseService, ITicketDbService
    {
        public TicketDbService(ABSContext context) : base(context)
        {
        }

        public void CreateTicket(int flightId, string seatId, string userId, string passangerName)
        {
            var flight = this.Context.Flights.FirstOrDefault(f => f.Id==flightId);
            if (flight == null)
                throw new ArgumentException("Flight does not exist");

            var seat = this.Context.Seats.FirstOrDefault(s => s.Id ==int.Parse(seatId));
            if (seat == null)
                throw new ArgumentException("Seat could not be found");

            var user = this.Context.Users.FirstOrDefault( u => u.Id == int.Parse(userId));
            if (user == null)
                throw new ArgumentException("User could not be found");

            seat.IsBooked = true;

            var ticket = new Ticket()
            {
                User = user,
                Seat = seat,
                Flight = flight,
                PassengerName = passangerName
            };


            this.Context.Tickets.Add(ticket);
            this.Context.SaveChanges();

        }

        public ICollection<Ticket> GetUserTickets(string userId)
        {
            return this.Context.Tickets
                .Where(t => t.UserId == int.Parse(userId))
                .Include(t => t.Flight).ThenInclude(f => f.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f.DestinationAirport)
                .Include(t => t.Flight).ThenInclude(f => f.Airline)
                .Include(t => t.Seat).ThenInclude(s => s.Section)
                .Include(t => t.User)
                .ToList();
        }
    }
}
