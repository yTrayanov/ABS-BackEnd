using ABS_Common.ResponsesModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ABS_Gateway.Common
{
    public class AuthorizeEndPointAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            AuthClient authClient = CreateAuthClient(context.HttpContext);
            ObjectResult result = await authClient.CheckAuthorized(context.HttpContext);

            if (result?.StatusCode == (int)HttpStatusCode.OK)
                await next();
            else
            {
                var response =
                context.Result = new UnauthorizedObjectResult(new ResponseObject( "User is not logged"));
            }
        }

        private AuthClient CreateAuthClient(HttpContext context)
        {
            var httpClientFactory = context.RequestServices.GetService(typeof(IHttpClientFactory)) as IHttpClientFactory;
            HttpClient client = httpClientFactory.CreateClient(APIUrls.AUTH);

            return new AuthClient(client);
        }
    }
}
