using Abs_SectionAirlineAirport.Models;
using ABS_Services.Interfaces;
using AirlineBookingSystem.Models;
using ABS_Common.ResponsesModels;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Abs_SectionAirlineAirport.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CreateController : ControllerBase
    {
        private ICreateDbService _createService;
        public CreateController(ICreateDbService service)
        {
            this._createService = service;
        }
        [HttpPost("section")]
        public IActionResult CreateSection([FromBody] SectionBindingModel sectionInfo)
        {
            try
            {
                var seatClass = sectionInfo.SeatClass.ToLower() == "first" ? SeatClass.First : sectionInfo.SeatClass.ToLower() == "bussiness" ? SeatClass.Bussiness : SeatClass.Economy;

                this._createService.CreateSection(sectionInfo.Rows, sectionInfo.Columns, seatClass , sectionInfo.FlightNumber);

                return new OkObjectResult(new ResponseObject(true, "Section created"));
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseObject(false, "Something went wrong", e.Message));
            }
        }
    }
}
