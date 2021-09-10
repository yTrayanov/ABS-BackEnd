namespace ABS_Tickets.Models
{
    public class TicketCreateModel
    {
        public int[] FlightIds { get; set; }
        public SeatBindingModel[][] Seats { get; set; }
    }
}
