using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AirlineBookingSystem.Models
{
    public enum UserStatus
    {
        Registered,
        LoggedIn
    }
    public class User : IdentityUser
    {
        public User()
        {
            this.Tickets = new List<Ticket>();
        }

        public ICollection<Ticket> Tickets { get; set; }

        public UserStatus Status { get; set; }
    }
}
