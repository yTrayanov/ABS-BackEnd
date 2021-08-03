using AirlineBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Tickets.Models
{
    public class TicketCreateModel
    {
        public int[] FlightIds { get; set; }
        public Seat[][] Seats { get; set; }
    }
}
