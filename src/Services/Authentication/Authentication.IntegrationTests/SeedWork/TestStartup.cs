using System.Data.Common;
using Authentication.Api;
using Authentication.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.IntegrationTests.SeedWork
{
    class TestStartup : Startup
    {
        public TestStartup(IWebHostEnvironment environment, IConfiguration configuration) : base(environment, configuration)
        {
        }

        protected override void AddDatabase(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(CreateInMemoryDatabase()));
        }
        
        public static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:;cache=shared");

            connection.Open();

            return connection;
        }
    }
}
