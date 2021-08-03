using AirlineBookingSystem.Data.Interfaces;
using AirlineBookingSystem.Models;
using System;
using System.Linq;

namespace AirlineBookingSystem.Data.Services
{
    public class CreateDbService : BaseService, ICreateDbService
    {
        public CreateDbService(ABSContext context) : base(context)
        {
        }

        public void CreateSection(int rows, int columns, SeatClass seatClass, string flightNumber)
        {
            var flight = this.Context.Flights.FirstOrDefault(f => f.FlightNumber == flightNumber);

            if (flight == null)
                throw new ArgumentException("Flight with this number was not found");

            var section = new Section()
            {
                Rows = rows,
                Columns = columns,
                SeatClass = seatClass,
                Flight = flight,
                AvailableSeatsCount = rows*columns
            };

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    var seat = new Seat()
                    {
                        Row = row + 1,
                        Col = col + 1,
                        Section = section
                    };
                    this.Context.Seats.Add(seat);
                }
            }

            this.Context.Sections.Add(section);
            this.Context.SaveChanges();

        }
    }
}
