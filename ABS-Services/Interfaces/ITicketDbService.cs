using AirlineBookingSystem.Models;
using System.Collections.Generic;

namespace ABS_Services.Interfaces
{
    public interface ITicketDbService
    {
        void CreateTicket(int flightId , string seatId , string userId , string passangerName);

        ICollection<Ticket> GetUserTickets(string userId);
    }
}
