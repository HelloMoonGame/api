using System;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Character.Api.Configuration
{
    public static class LoggingExtensions
    {
        internal static LoggerConfiguration ConfigureBaseLogging(
            this LoggerConfiguration loggerConfiguration,
            IConfiguration configuration,
            string appName,
            Version appVersion
        )
        {
            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("ApplicationVersion", appVersion.ToString())
                .Enrich.WithProperty("ApplicationName", appName);

            return loggerConfiguration;
        }
    }
}
