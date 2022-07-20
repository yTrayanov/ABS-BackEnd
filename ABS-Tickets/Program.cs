using ABS.Data.DynamoDbRepository;
using ABS_Common.Extensions;
using ABS_Tickets.Models;
using ABS_Tickets.Repository;
using ABS_Tickets.Service;
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ABS_Tickets", Version = "v1" });
});

builder.Services.AddSingleton<IRepository<string, TicketModel>, TicketRepository>();
builder.Services.AddTransient<ITicketService, TicketeService>();

builder.Services.ConfigureJwt(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ABS_Tickets v1"));
}

app.ConfigureExceptionHandler();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


