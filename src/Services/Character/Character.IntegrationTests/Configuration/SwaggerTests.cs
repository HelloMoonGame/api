using System.Threading.Tasks;
using Character.IntegrationTests.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Character.IntegrationTests.Configuration
{
    [TestClass]
    public class SwaggerTests : WebHostTestBase
    {
        [TestMethod]
        public async Task Swagger_configuration_is_valid()
        {
            // Act
            var response = await Client.GetAsync("/swagger/v1/swagger.json");
            var content = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(content);
            
            // Assert
            Assert.IsNotNull(root);
        }
    }
}
