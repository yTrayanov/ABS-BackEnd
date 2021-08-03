using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ABS_Gateway.Common
{
    public class Client : IClient
    {
        protected readonly HttpClient ApiClient;

        public Client(HttpClient client)
        {
            ApiClient = client;
        }

        public string BaseAddress { get; set; }

        public async Task<ObjectResult> Get(HttpContext context, string endPoint = "")
        {
            return await ForwardToAPI(context, null, endPoint);
        }

        public async Task<ObjectResult> Post(HttpContext context, object body, string endPoint = "")
        {
            return await ForwardToAPI(context, body, endPoint);
        }

        private async Task<ObjectResult> ForwardToAPI(HttpContext context, object body = null, string endPoint = "")
        {
            const string AUTH_HEADER = "Authorization";
            string requestUri;
            HttpRequest request = context.Request;
            HttpResponseMessage response;

            if (string.IsNullOrEmpty(endPoint))
               requestUri = request.Path + request.QueryString.Value;
            else
                requestUri = endPoint;

            if (ApiClient.BaseAddress == null)
                ApiClient.BaseAddress = new Uri(BaseAddress);

            string accessToken = request.Headers[AUTH_HEADER];
            if (!string.IsNullOrEmpty(accessToken))
                ApiClient.DefaultRequestHeaders.Add(AUTH_HEADER, accessToken);

            if (body == null)
            {
                response = await ApiClient.GetAsync(requestUri);
            }
            else
            {
                JsonContent content = JsonContent.Create(body);
                response = await ApiClient.PostAsync(requestUri, content);
            }

            string jsonModel = await response.Content?.ReadAsStringAsync();
            return new ObjectResult(jsonModel) { StatusCode = (int)response.StatusCode };
        }
    }
}
