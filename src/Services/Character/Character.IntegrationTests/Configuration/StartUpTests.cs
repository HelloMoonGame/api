using System;
using System.Threading.Tasks;
using Character.Api;
using Character.IntegrationTests.SeedWork;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Character.IntegrationTests.Configuration
{
    [TestClass]
    public class StartUpTests
    {
        [TestMethod]
        public async Task Application_shows_no_exception_details_in_production_mode()
        {
            // Arrange
            using var factory = new CustomWebApplicationFactory<Startup>(Environments.Production);
            using var client = factory.CreateClient();
            WebHostTestBase.AuthenticateWith(client, "not-a-guid");

            // Assert
            await Assert.ThrowsExceptionAsync<FormatException>(() => client.GetAsync("/mycharacter"));
        }
        
        [TestMethod]
        public async Task Application_shows_exception_details_in_development_mode()
        {
            // Arrange
            using var factory = new CustomWebApplicationFactory<Startup>(Environments.Development);
            using var client = factory.CreateClient();
            WebHostTestBase.AuthenticateWith(client, "not-a-guid");

            // Act
            var response = await client.GetAsync("/mycharacter");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.IsTrue(content.Contains("System.FormatException"));
        }
    }
}
