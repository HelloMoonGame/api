using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Character.Api;
using Character.Api.Configuration;
using Character.IntegrationTests.SeedWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;

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
