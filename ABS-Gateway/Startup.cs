using ABS_Gateway.Common;
using AirlineBookingSystem.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ABS_Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            IConfigurationSection apiUrlSection = Configuration.GetSection(nameof(APIUrls));
            services.Configure<APIUrls>(apiUrlSection);

            services.AddHttpClient<IClient, Client>();


            services.AddDbContext<ABSContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("AbsContext")));

            APIUrls apiUrls = apiUrlSection.Get<APIUrls>();
            services.AddHttpClient(APIUrls.AUTH, c => c.BaseAddress = new Uri(apiUrls.AuthApi));

            APIUrls apiUrl = apiUrlSection.Get<APIUrls>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TBSGatewayAPI", Version = "v1" });
            });



        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
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

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TBSGatewayAPI v1"));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
