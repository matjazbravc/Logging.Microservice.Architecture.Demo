using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using System.Reflection;

namespace Services.Gateway
{
    public class Startup
    {
        private readonly string _service_name;

        public Startup(IConfiguration configuration)
        {
            _service_name = Assembly.GetExecutingAssembly().GetName().Name;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddOcelot()
                .AddPolly()
                .AddCacheManager(x =>
                {
                    x.WithDictionaryHandle();
                });
            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature.Error;
                logger.LogError(exception.Message);
                var result = JsonConvert.SerializeObject(new { error = exception.Message });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);
            }));
            
            app.UseCors(b => b
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );
            
            app.UseRouting();
            app.UseStaticFiles();
            
            app.UseEndpoints(config =>
            {
                config.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync(_service_name);
                });
                config.MapGet("/info", async context =>
                {
                    await context.Response.WriteAsync($"{_service_name}, running on {context.Request.Host}");
                });
            });
            
            app.UseOcelot().Wait();
        }
    }
}
