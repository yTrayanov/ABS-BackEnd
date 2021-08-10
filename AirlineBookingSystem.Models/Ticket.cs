using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AirlineBookingSystem.Models
{
    public class Ticket:BaseModel
    {

        public int FlightId { get; set; }
        public Flight Flight { get; set; }

        public int SeatId { get; set; }
        public Seat Seat { get; set; }

        
        [Required]
        public string PassengerName { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
