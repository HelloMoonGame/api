using System;
using Character.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Character.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            LoggingExtensions.SetupLoggerConfiguration(AppName, AppVersion);

            try
            {
                Log.Information("Starting web host for {0}", AppName);
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostBuilderContext, loggerConfiguration) => {
                    loggerConfiguration.ConfigureBaseLogging(
                        hostBuilderContext.Configuration, AppName, AppVersion);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        
        public static string AppName => typeof(Program).Assembly.GetName().Name;
        public static Version AppVersion => typeof(Program).Assembly.GetName().Version;
    }
}
