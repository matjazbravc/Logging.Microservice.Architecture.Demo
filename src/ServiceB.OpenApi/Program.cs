using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Serilog;
using System.IO;
using System.Reflection;
using System;

namespace ServiceB.OpenApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHost(args);
        }

        private static void CreateHost(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal($"Failed to start {Assembly.GetExecutingAssembly().GetName().Name}", ex);
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                    webBuilder.UseSerilog((hostingContext, config) => config
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails()
                        .Enrich.WithMachineName()
                        .Enrich.WithCorrelationId()
                        .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                        .WriteTo.Console()
                        .WriteTo.Elasticsearch(ConfigureElasticSearchSink())
                        .ReadFrom.Configuration(hostingContext.Configuration));
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((builderContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddSerilog();
                });

        private static ElasticsearchSinkOptions ConfigureElasticSearchSink()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .Build();
            var elasticUri = new Uri(configuration["ElasticConfiguration:Uri"]);
            return new ElasticsearchSinkOptions(elasticUri)
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"logging-microservice-architecture-demo-{DateTime.UtcNow:yyyy-MM}"
            };
        }
    }
}
