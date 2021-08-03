using AirlineBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineBookingSystem.Data.Interfaces
{
    public interface ICreateDbService
    {
        void CreateSection(int rows , int columns , SeatClass seatClass , string flightNubmer);
    }
}
