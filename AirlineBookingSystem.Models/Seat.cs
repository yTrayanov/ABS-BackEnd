using System;
using System.ComponentModel.DataAnnotations;

namespace AirlineBookingSystem.Models
{
    public class Seat:BaseModel
    {
        public Seat()
        {
            this.IsBooked = false;
        }

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

        public int SectionId { get; set; }
        public Section Section { get; set; }

        public int? TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public string PassangerName { get; set; }

    }
}
