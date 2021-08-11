using Abs_SectionAirlineAirport.Models;
using AirlineBookingSystem.Models;
using ABS_Common.ResponsesModels;
using Microsoft.AspNetCore.Mvc;
using System;
using AirlineBookingSystem.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abs_SectionAirlineAirport.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CreateController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        public CreateController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        [HttpPost("section")]
        public async  Task<IActionResult> CreateSection([FromBody] SectionBindingModel sectionInfo)
        {
            try
            {
                var flight = await _unitOfWork.Flights.Get(f => f.FlightNumber == sectionInfo.FlightNumber);

                if (flight == null)
                    throw new ArgumentException("Flight does not exist");

                var seatClass = sectionInfo.SeatClass.ToLower() == "first" ? SeatClass.First : sectionInfo.SeatClass.ToLower() == "bussiness" ? SeatClass.Bussiness : SeatClass.Economy;

                if (flight.Sections.Any(s => s.SeatClass == seatClass))
                    throw new ArgumentException("Flight already contains seat class");


                var section = new Section
                {
                    Rows = sectionInfo.Rows,
                    Columns = sectionInfo.Columns,
                    SeatClass = sectionInfo.SeatClass.ToLower() == "first" ? SeatClass.First : sectionInfo.SeatClass.ToLower() == "bussiness" ? SeatClass.Bussiness : SeatClass.Economy,
                    FlightId = flight.Id,
                    AvailableSeatsCount = sectionInfo.Rows * sectionInfo.Columns
                };

                var seats = new List<Seat>();

                for (int row = 0; row < section.Rows; row++)
                {
                    for (int col = 0; col < section.Columns; col++)
                    {
                        var seat = new Seat()
                        {
                            Row = row + 1,
                            Col = col + 1,
                            Section = section
                        };
                        seats.Add(seat);
                    }
                }

                await _unitOfWork.Sections.Insert(section);
                await _unitOfWork.Seats.InsertRange(seats);
                await _unitOfWork.Save();

                return new OkObjectResult(new ResponseObject(true, "Section created"));
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseObject(false, "Something went wrong", e.Message));
            }
        }
    }
}
