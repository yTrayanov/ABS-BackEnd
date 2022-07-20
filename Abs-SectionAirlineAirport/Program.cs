using ABS.Data.DynamoDbRepository;
using ABS_Common.Extensions;
using Abs_SectionAirlineAirport.Models;
using Abs_SectionAirlineAirport.Repositories;
using Abs_SectionAirlineAirport.Service;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Abs_SectionAirlineAirport", Version = "v1" });
});

builder.Services.AddSingleton<IRepository<string, AirlineModel>, AirlineRepository>();
builder.Services.AddSingleton<IRepository<string, AirportModel>, AirportRepository>();
builder.Services.AddSingleton<IRepository<string, SectionModel>, SectionRepository>();
builder.Services.AddTransient<ICreateService, CreateService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Abs_SectionAirlineAirport v1"));
}

app.ConfigureExceptionHandler();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();


