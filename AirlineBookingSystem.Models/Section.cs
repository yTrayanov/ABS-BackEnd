﻿namespace AirlineBookingSystem.Models
{
    using AirlineBookingSystem.Models.Constants;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text.Json.Serialization;

    public class Section:BaseModel
    {



        public int AvailableSeatsCount { get; set; }

        public int Rows { get; set; }
        public int Columns { get; set; }
        public SeatClass SeatClass { get; set; }
        public Flight Flight { get; set; }

        public ICollection<Seat> Seats { get; set; }

        [NotMapped]
        public bool hasAvailableSeats
        {
            get
            {
                return this.AvailableSeatsCount > 0;
            }
        }

        public int FlightId { get; set; }
    }
}
