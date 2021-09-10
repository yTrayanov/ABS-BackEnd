namespace AirlineBookingSystem.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

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
