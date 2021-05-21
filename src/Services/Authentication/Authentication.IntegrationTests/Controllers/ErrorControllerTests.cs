using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using Authentication.IntegrationTests.Helpers;
using Authentication.IntegrationTests.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Authentication.IntegrationTests.Controllers
{
    [TestClass]
    public class ErrorControllerTests : WebHostTestBase
    {
        [TestMethod]
        public async Task The_homepage_shows_a_404_page()
        {
            var result = await Client.GetAsync("/");
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
        }
        
        [TestMethod]
        public async Task The_404_page_is_styled()
        {
            var result = await Client.GetAsync("/");
            var resultDocument = await result.GetDocumentAsync();
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
            Assert.IsNotNull(resultDocument.GetElementsByClassName("error-page"));
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
