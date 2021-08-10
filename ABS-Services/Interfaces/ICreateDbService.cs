using AirlineBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABS_Services.Interfaces
{
    public interface ICreateDbService
    {
        void CreateSection(int rows , int columns , SeatClass seatClass , string flightNubmer);
    }
}
