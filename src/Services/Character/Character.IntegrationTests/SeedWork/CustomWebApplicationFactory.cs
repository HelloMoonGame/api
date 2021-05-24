using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Character.Api.Configuration;
using Character.Api.Domain.Characters;
using Character.Api.Infrastructure.Database;
using Character.IntegrationTests.Mocks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Character.IntegrationTests.SeedWork
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {   
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseEnvironment("test")
                .ConfigureTestServices(ConfigureServices)
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            ["AuthenticationApiUrl"] = "https://auth"
                        });
                })
                .Configure(async app =>
                {
                    app.ConfigureApp(false);
                });
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            ReplaceDbContext(services);
        }

        private static void ReplaceDbContext(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<CharactersContext>));

            services.Remove(descriptor);

            var testDatabase = CreateInMemoryDatabase();
            services.AddDbContext<CharactersContext>(options =>
                options.UseSqlite(testDatabase));
        }
        
        public static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:;cache=shared");

            connection.Open();

            return connection;
        }
    }
}
