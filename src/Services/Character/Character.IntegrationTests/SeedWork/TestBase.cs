using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Character.Api;
using Character.Api.Application.CharacterLocations.DomainServices;
using Character.Api.Application.Characters.DomainServices;
using Character.Api.Domain.CharacterLocations;
using Character.Api.Domain.Characters;
using Character.Api.Domain.SeedWork;
using Character.Api.Infrastructure.Database;
using Character.Api.Infrastructure.Domain;
using Character.Api.Infrastructure.Domain.CharacterLocations;
using Character.Api.Infrastructure.Domain.Characters;
using Character.Api.Infrastructure.Processing;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

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