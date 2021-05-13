using System.Net;
using System.Threading.Tasks;
using Authentication.IntegrationTests.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Authentication.IntegrationTests.Controllers
{
    [TestClass]
    public class HomeControllerTests : WebHostTestBase
    {
        [TestMethod]
        public async Task There_is_no_homepage()
        {
            var result = await Client.GetAsync("/");
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
        }
    }
}
