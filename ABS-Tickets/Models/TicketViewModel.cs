using ABS_Common.Enumerations;

namespace ABS_Tickets.Models
{
    public class TicketViewModel
    {
        public string Id { get; set; }
        public string FlightId { get; set; }
        public Flight Flight { get; set; }

        public string SeatId { get; set; }
        public SeatModel Seat { get; set; }
        public string PassengerName { get; set; }

        public SeatClass SeatClass { get; set; }
    }
}
