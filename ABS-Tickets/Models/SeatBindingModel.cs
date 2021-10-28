using Abs.Common.CustomAttributes;

namespace ABS_Tickets.Models
{
    public class SeatBindingModel
    {
        public string Id { get; set; }

        [CustomRequired]
        public string PassengerName { get; set; }
    }
}
