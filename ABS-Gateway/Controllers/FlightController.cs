using ABS_Gateway.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABS_Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlightController : BaseController
    {
        public FlightController(IOptions<APIUrls> urls, IClient client) : base(urls, client)
        {
            this.Client.BaseAddress = urls.Value.FlightApi;
        }

        [HttpGet("filter/{OriginAirport}/{DestinationAirport}/{DepartureDate}/{MembersCount}")]
        public async Task<IActionResult> FilterOneWayFlights()
        {
            return await Client.Get(HttpContext);
        }

        [HttpGet("filter/{OriginAirport}/{DestinationAirport}/{DepartureDate}/{MembersCount}/{ReturnDate}")]
        public async Task<IActionResult> FilterTwoWayFlights()
        {
            return await Client.Get(HttpContext);
        }

        [HttpGet("{multipleIds}")]
        public async Task<IActionResult> GetFlightsByIds()
        {
            return await Client.Get(HttpContext);
        }

        [HttpPost("create")]
        [AuthorizeAdminEndPoint]
        public async Task<IActionResult> CreateFlight([FromBody] object body)
        {
            return await Client.Post(HttpContext, body);
        }


        [HttpGet("information/all")]
        public async Task<IActionResult> GetAllFlights()
        {
            return await Client.Get(HttpContext);
        }

        [HttpGet("information/{Id}")]
        public async Task<IActionResult> GetFlightInformation()
        {
            return await Client.Get(HttpContext);
        }
    }
}
