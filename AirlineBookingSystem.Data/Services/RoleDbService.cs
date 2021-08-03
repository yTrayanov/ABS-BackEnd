using AirlineBookingSystem.Data.Interfaces;
using AirlineBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AirlineBookingSystem.Data.Services
{
    public class RoleDbService : BaseService , IRoleDbService
    {
        public RoleDbService(ABSContext context) : base(context)
        {
        }

        public void AddRoleToUser(User user, string roleName)
        {
            var role = this.GetRole(roleName);

            var userRole = new UserRole() { Role = role, User = user };

            this.Context.UserRoles.Add(userRole);
            this.Context.SaveChanges();
        }

        public List<Role> GetUserRoles(User user)
        {
            return this.Context.Roles.Where(r => this.Context.UserRoles.Any(ur => ur.Role == r && ur.User == user)).ToList();
        }

        private Role GetRole(string name)
        {
            return this.Context.Roles.FirstOrDefault(r => r.Name == name);
        }
    }
}
