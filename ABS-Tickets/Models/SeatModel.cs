using ABS_Common.Enumerations;
using System;

namespace ABS_Tickets.Models
{
    public class SeatModel
    {
        public string Id { get; set; }
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

        public SeatClass SeatClass { get; set; }

        public string FlightId { get; set; }

        public string SectionId { get; set; }

    }
}
