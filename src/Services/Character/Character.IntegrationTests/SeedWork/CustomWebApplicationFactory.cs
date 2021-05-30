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
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Character.IntegrationTests.SeedWork
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _environment;

        public CustomWebApplicationFactory(string environment = null)
        {
            _environment = environment ?? Environments.Production;
        }
        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseEnvironment(_environment)
                .ConfigureTestServices(ConfigureServices)
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            ["AuthenticationApiUrl"] = "https://auth"
                        });
                })
                .Configure(async (context, app) =>
                {
                    app.ConfigureApp(context.HostingEnvironment.IsEnvironment(Environments.Development));

                    var db = app.ApplicationServices.GetService<CharactersContext>();
                    if (db == null)
                        throw new Exception("CharactersContext not found!");
                    
                    await db.Characters.AddAsync(Api.Domain.Characters.Character.Create(
                        Guid.Parse(WebHostTestBase.UserIdWithCharacter),
                        "Hello",
                        "Moon",
                        SexType.Male,
                        app.ApplicationServices.GetService<ISingleCharacterPerUserChecker>()
                    ));
                    await db.SaveChangesAsync();
                });
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            ReplaceDbContext(services);
            ReplaceAuthentication(services);
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

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:;cache=shared");

            connection.Open();

            return connection;
        }

        private static void ReplaceAuthentication(IServiceCollection services)
        {
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var config = new OpenIdConnectConfiguration()
                {
                    Issuer = MockJwtTokens.Issuer
                };

                config.SigningKeys.Add(MockJwtTokens.SecurityKey);
                options.Configuration = config;
            });
        }
    }
}
