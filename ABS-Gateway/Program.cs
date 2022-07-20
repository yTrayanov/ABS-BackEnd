using ABS_Common.Extensions;
using ABS_Gateway.Common;
using Microsoft.OpenApi.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

IConfigurationSection apiUrlSection = builder.Configuration.GetSection(nameof(APIUrls));

builder.Services.Configure<APIUrls>(apiUrlSection);

builder.Services.AddHttpClient<IClient, Client>();


APIUrls apiUrls = apiUrlSection.Get<APIUrls>();

builder.Services.AddHttpClient(APIUrls.AUTH, c => c.BaseAddress = new Uri(apiUrls.AuthApi));


APIUrls apiUrl = apiUrlSection.Get<APIUrls>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TBSGatewayAPI", Version = "v1" });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Use(async (context, nextMiddleware) =>
{
    context.Response.OnStarting(() =>
    {
        var allowOrigin = "Access-Control-Allow-Origin";
        var allowHeaders = "Access-Control-Allow-Headers";
        var allowMethods = "Access-Control-Allow-Methods";
        var allowMaxAge = "Access-Control-Max-Age";
        var allowCredentials = "Access-Control-Allow-Credentials";

        if (!context.Response.Headers.ContainsKey(allowOrigin))
            context.Response.Headers.Add(allowOrigin, "http://localhost:3000");
        if (!context.Response.Headers.ContainsKey(allowHeaders))
            context.Response.Headers.Add(allowHeaders, "origin, x-requested-with, accept, content-type, authorization");
        if (!context.Response.Headers.ContainsKey(allowMethods))
            context.Response.Headers.Add(allowMethods, "GET, PUT, POST, DELETE, OPTIONS");
        if (!context.Response.Headers.ContainsKey(allowMaxAge))
            context.Response.Headers.Add(allowMaxAge, "3628800");
        if (!context.Response.Headers.ContainsKey(allowCredentials))
            context.Response.Headers.Add(allowCredentials, "true");

        if (context.Request.Method == "OPTIONS")
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        return Task.FromResult(0);
    });
    await nextMiddleware();
});


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.CreateDatabaseTables(builder.Configuration);

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TBSGatewayAPI v1"));

app.MapControllers();

app.Run();


