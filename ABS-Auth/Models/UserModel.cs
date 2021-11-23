using ABS_Auth.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Auth.Models
{
    public class UserModel
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public UserStatus Status { get; set; }

        public string Roles { get; set; }
    }
}
