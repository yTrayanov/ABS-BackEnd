using ABS_Common.Enumerations;

namespace ABS_Tickets.Models
{
    public class TicketModel
    {
        public string Id { get; set; }
        public string FlightId { get; set; }
        public Flight Flight { get; set; }

        public string SeatId { get; set; }
        public SeatModel Seat { get; set; }
        public string PassengerName { get; set; }
        public string Username { get; set; }

        public SeatClass SeatClass { get; set; }

        public string SectionId { get; set; }
    }
}
