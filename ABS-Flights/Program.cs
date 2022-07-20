using ABS.Data.DynamoDbRepository;
using ABS_Common.Extensions;
using ABS_Flights.Models;
using ABS_Flights.Repository;
using ABS_Flights.Service;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMvc().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ABS-Flights-Api", Version = "v1" });
});

builder.Services.AddSingleton<IRepository<string, FlightModel>, FlightRepository>();
builder.Services.AddTransient<IFlightService, FlightService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ABS-Flights-Api v1"));
}

app.ConfigureExceptionHandler();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();


