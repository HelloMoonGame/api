using System.Threading.Tasks;
using Character.IntegrationTests.SeedWork;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Character.IntegrationTests.Configuration
{
    [TestClass]
    public class SwaggerTests
    {
        [TestMethod]
        public async Task Swagger_configuration_is_validAsync()
        {
            var webHostBuilder =
                new WebHostBuilder()
                    .UseEnvironment("Test")
                    .UseStartup<TestStartup>();

            JObject root;
            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            {
                var result = await client.GetStringAsync("/swagger/v1/swagger.json");
                root = JObject.Parse(result);
            }

            Assert.IsNotNull(root);
        }
    }
}
