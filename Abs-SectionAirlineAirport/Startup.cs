using ABS_Data.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ABS_Common.Extensions;
using Abs_SectionAirlineAirport.Service;
using ABS.Data.DynamoDbRepository;
using Abs_SectionAirlineAirport.Models;
using Abs_SectionAirlineAirport.Repositories;

namespace Abs_SectionAirlineAirport
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Abs_SectionAirlineAirport", Version = "v1" });
            });


            services.AddSingleton<IRepository<string, AirlineModel>, AirlineRepository>();
            services.AddSingleton<IRepository<string, AirportModel>, AirportRepository>();
            services.AddSingleton<IRepository<string, SectionModel>, SectionRepository>();
            services.AddTransient<ICreateService, CreateService>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Abs_SectionAirlineAirport v1"));
            }

            app.ConfigureExceptionHandler();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
