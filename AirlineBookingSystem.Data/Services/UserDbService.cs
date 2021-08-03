using AirlineBookingSystem.Data.Interfaces;
using AirlineBookingSystem.Models;
using System;
using System.Linq;

namespace AirlineBookingSystem.Data.Services
{
    public class UserDbService : BaseService, IUserDbService
    {
        public UserDbService(ABSContext context) : base(context)
        {
        }

        public User GetUser(string id)
        {
            var user = this.Context.Users.FirstOrDefault(u => u.Id == int.Parse(id)); ;
            if (user == null)
                throw new ArgumentException("User does not exist");
            return user;
        }

        public User LoginUser(string username, string password)
        {
            var user = this.Context.Users.FirstOrDefault(u => u.Username == username && u.HashedPassword == password);
            if (user == null)
                throw new ArgumentException("Invalid username or password");

            user.Status = UserStatus.LoggedIn;
            this.Context.SaveChanges();

            return user;
        }

        public void LogoutUser(string id)
        {
            var user = this.GetUser(id);

            if (user.Status == UserStatus.Registered)
                return;

            user.Status = UserStatus.Registered;
            this.Context.SaveChanges();
        }

        public void RegisterUser(User user)
        {
            if (this.Context.Users.Any(u => u.Email == user.Email))
                throw new ArgumentException("Email already in use");
            if (this.Context.Users.Any(u => u.Username == user.Username))
                throw new ArgumentException("Username is already taken");


            this.Context.Users.Add(user);
            this.Context.SaveChanges();
        }
    }
}
