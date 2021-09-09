namespace AirlineBookingSystem.Models
{
    public class Ticket:BaseModel
    {
        public int FlightId { get; set; }
        public Flight Flight { get; set; }

        public int SeatId { get; set; }
        public Seat Seat { get; set; }


        public string PassengerName { get; set; }
        public string Username { get; set; }
    }
}
