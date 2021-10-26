﻿namespace ABS_Flights.Models
{
    using ABS_Common.Enumerations;
    using System.Collections.Generic;

    public class Section
    {
        public string Id { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public SeatClass SeatClass { get; set; }

        public ICollection<Seat> Seats { get; set; }
        public string FlightId { get; set; }
    }
}
