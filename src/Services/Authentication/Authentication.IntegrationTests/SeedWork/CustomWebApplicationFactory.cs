using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using Authentication.Api;
using Authentication.Api.Data;
using Authentication.Api.Infrastructure;
using Authentication.Api.Models;
using Authentication.Api.Services;
using Authentication.IntegrationTests.Mocks;
using IdentityModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.IntegrationTests.SeedWork
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseEnvironment("test")
                .ConfigureServices(services =>
                {
                    ReplaceDbContext(services);
                    ReplaceMailService(services);
                })
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            ["AuthenticationApiUrl"] = "http://localhost"
                        });
                })
                .Configure(app =>
                {
                    app.ConfigureApp(false);
                    
                    var userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
                    if (userManager == null)
                        throw new Exception("UserManager not found!");
                            
                    var alice = new ApplicationUser
                    {
                        UserName = "alice",
                        Email = "AliceSmith@email.com",
                        EmailConfirmed = true,
                    };
                    
                    var result = userManager.CreateAsync(alice).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userManager.AddClaimsAsync(alice, new Claim[]{
                        new (JwtClaimTypes.Email, alice.Email)
                    }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                });
        }

        private static void ReplaceDbContext(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<ApplicationDbContext>));

            services.Remove(descriptor);

            var testDatabase = CreateInMemoryDatabase();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(testDatabase));
        }

        private static void ReplaceMailService(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IMailService));

            services.Remove(descriptor);
            
            services.AddScoped<IMailService, MailServiceMock>();
        }

        public static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:;cache=shared");

            connection.Open();

            return connection;
        }
    }
}
