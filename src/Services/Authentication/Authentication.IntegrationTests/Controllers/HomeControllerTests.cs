using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using Authentication.IntegrationTests.Helpers;
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
        
        [TestMethod]
        public async Task Display_nice_error_on_exception()
        {
            var result = await Client.GetLoginPage("test", "http://localhost");
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsNotNull(result.GetElementsByClassName("error-page"));
            Assert.IsTrue(result.ToHtml().Contains("unauthorized_client"), "Output should contain 'unauthorized_client', but was: " + result.ToHtml());
        }
    }
}
