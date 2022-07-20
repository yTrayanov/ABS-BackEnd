using ABS.Data.DynamoDbRepository;
using ABS_Auth.Common;
using ABS_Auth.Models;
using ABS_Auth.Repository;
using ABS_Common.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ABS-Auth-Api", Version = "v1" });
});

builder.Services.AddSingleton<IRepository<string, UserModel>, UserRepository>();
builder.Services.AddTransient<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ABS-Auth-Api v1"));
}

app.ConfigureExceptionHandler();
app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


