using AirlineBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineBookingSystem.Data.Interfaces
{
    public interface IRoleDbService
    {
        void AddRoleToUser(User user , string role);
        public List<Role> GetUserRoles(User user);
    }
}
