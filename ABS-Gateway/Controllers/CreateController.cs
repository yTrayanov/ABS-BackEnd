using ABS_Gateway.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ABS_Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AuthorizeAdminEndPoint]
    public class CreateController : BaseController
    {
        public CreateController(IOptions<APIUrls> urls, IClient client) : base(urls, client)
        {
            this.Client.BaseAddress = urls.Value.CreateApi;
        }


        [HttpPost("section")]
        public async Task<IActionResult> CreateSection([FromBody] object body)
        {
            return await Client.Post(HttpContext, body);
        }

    }
}
