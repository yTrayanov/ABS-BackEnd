using ABS_Gateway.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ABS_Gateway.Controllers
{
    public class BaseController : ControllerBase
    {
        public BaseController(IOptions<APIUrls> urls , IClient client)
        {
            this.Urls = urls;
            this.Client = client;
        }
        public IClient Client { get; set; }
        public IOptions<APIUrls> Urls { get; set; }
    }
}
