using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABS_Gateway.Common
{
    public interface IClient
    {
        string BaseAddress { get; set; }
        Task<ObjectResult> Get(HttpContext context, string endPoint = "");
        Task<ObjectResult> Post(HttpContext context, object body, string endPoint = "");
    }
}
