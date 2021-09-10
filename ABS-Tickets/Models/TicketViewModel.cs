namespace ABS_Tickets.Models
{
    public class TicketViewModel
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public Flight Flight { get; set; }

        public int SeatId { get; set; }
        public SeatModel Seat { get; set; }
        public string PassengerName { get; set; }
    }
}
