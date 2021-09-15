using Abs.Common.CustomAttributes;

namespace ABS_Tickets.Models
{
    public class TicketCreateModel
    {
        [CustomRequired]
        public int[] FlightIds { get; set; }

        [CustomRequired]
        public SeatBindingModel[][] Seats { get; set; }
    }
}
