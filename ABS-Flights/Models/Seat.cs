using ABS_Common.Enumerations;
using System;

namespace ABS_Flights.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsBooked { get; set; } 
        public string SeatNumber
        {
            get
            {
                return this.Row.ToString() + Convert.ToChar(Column - 1 + 'A');
            }
        }

        public int SectionId { get; set; }

        public string Username { get; set; }

        public SeatClass SeatClass { get; set; }
        public string PassengerName { get; set; }

    }
}
