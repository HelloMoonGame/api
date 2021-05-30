using System;
using System.Threading.Tasks;
using Authentication.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Authentication.Api
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await RunWithLogging(() => CreateHostBuilder(args).Build().RunAsync());
        }
        
        public static async Task<int> RunWithLogging(Func<Task> action)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting web host");
                await action();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.Information("Host terminated");
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