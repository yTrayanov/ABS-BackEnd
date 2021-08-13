using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABS_Gateway.Common
{
    public interface IAuthClient
    {
        Task<ObjectResult> CheckAuthorized(HttpContext context);
        Task<ObjectResult> CheckAdminAuthorized(HttpContext context);
    }
}
