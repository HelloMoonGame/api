using System.Data.Common;
using Character.Api;
using Character.Api.Infrastructure.Database;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Character.IntegrationTests.SeedWork
{
    public abstract class TestBase
    {
        protected ServiceProvider Services;

        protected TestBase()
        {
            var services = new ServiceCollection();
            
            services.AddDbContext<CharactersContext>(options =>
                options.UseSqlite(CreateInMemoryDatabase()));
            services.AddScoped<DbContext>(provider => provider.GetService<CharactersContext>());

            Startup.AddDependencies(services);
            
            services.AddLogging();

            services.AddMediatR(typeof(Startup));
            
            Services = services.BuildServiceProvider();

            var database = Services.GetService<CharactersContext>();
            database?.Database.Migrate();
        }

        public static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:;cache=shared");

            connection.Open();
            
            return connection;
        }
    }
}