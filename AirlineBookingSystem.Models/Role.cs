using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineBookingSystem.Models
{
    public class Role:BaseModel
    {
        public Role()
        {
            this.Users = new List<UserRole>();
        }
        public string Name { get; set; }

        public int Authority { get; set; }

        public ICollection<UserRole> Users { get; set; }
    }
}
