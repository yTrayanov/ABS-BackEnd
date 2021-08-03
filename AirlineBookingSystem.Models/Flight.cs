namespace AirlineBookingSystem.Models
{
    using AirlineBookingSystem.Models.CustomAttributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Flight:BaseModel
    {
        public Flight()
        {
            this.Sections = new List<Section>();
            this.Tickets = new List<Ticket>();
        }

        [Required(ErrorMessage = "Flight id is required")]
        public string FlightNumber { get; set; }

        public int AirlineId { get; set; }
        public Airline Airline { get; set; }

        public Airport OriginAirport { get; set; }
        public int OriginAirportId { get; set; }


        [NotEqual(nameof(OriginAirport))]
        public Airport DestinationAirport { get; set; }
        public int DestinationAirportId { get; set; }

        public ICollection<Section> Sections { get; set; }


        [NotPastDate]
        public DateTime DepartureDate { get; set; }


        [NotPastDate]
        public DateTime LandingDate {get;set; }

        public bool IsAvailable
        {
            get
            {
                foreach (var section in this.Sections)
                {
                    if (section.hasAvailableSeats)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public ICollection<Ticket> Tickets { get; set; }
    }
}
