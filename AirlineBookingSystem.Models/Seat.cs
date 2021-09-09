using System;
using System.ComponentModel.DataAnnotations;

namespace AirlineBookingSystem.Models
{
    public class Seat:BaseModel
    {

        [Required]
        public int Row { get; set; }
        public int Col { get; set; }
        public bool IsBooked { get; set; } 
        public string SeatNumber
        {
            get
            {
                return this.Row.ToString() + Convert.ToChar(Col - 1 + 'A');
            }
        }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public int SectionId { get; set; }

        public string Username { get; set; }

        public SeatClass SeatClass { get; set; }
        public string PassengerName { get; set; }

    }
}
