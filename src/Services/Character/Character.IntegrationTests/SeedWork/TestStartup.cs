using Character.Api;
using Character.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Character.IntegrationTests.SeedWork
{
    class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration) : base(configuration)
        {
            configuration["AuthenticationApiUrl"] = "https://localhost:5000";
        }

        protected override void AddDatabase(IServiceCollection services)
        {
            services.AddDbContext<CharactersContext>(options =>
                options.UseSqlite(TestBase.CreateInMemoryDatabase()));
        }
    }
}
