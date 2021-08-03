using AirlineBookingSystem.Models;
using System;

namespace AirlineBookingSystem.Data.Interfaces
{
    public interface IUserDbService
    {
        User GetUser(string id);
        User LoginUser(string username, string password );

        void LogoutUser(string id);

        void RegisterUser(User user);
        
    }
}
