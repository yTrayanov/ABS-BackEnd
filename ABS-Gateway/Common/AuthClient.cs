using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ABS_Gateway.Common
{
    public class AuthClient : Client , IAuthClient
    {
        public AuthClient(HttpClient client) : base(client)
        {
        }

        public async Task<ObjectResult> CheckAuthorized(HttpContext context)
        {
            return await Get(context, APIUrls.AUTH_ENDPOINT);
        }
    }
}
