using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AirlineBookingSystem.Models
{
    public enum UserStatus
    {
        Registered,
        LoggedIn
    }
    public class User : BaseModel
    {
        public User()
        {
            this.Tickets = new List<Ticket>();
            this.Roles = new List<UserRole>();
        }


        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }
        public byte[] Salt { get; set; }

        [Required]
        public string HashedPassword { get; set; }
        public ICollection<UserRole> Roles { get; set; }

        public ICollection<Ticket> Tickets { get; set; }

        public UserStatus Status { get; set; }
    }
}
