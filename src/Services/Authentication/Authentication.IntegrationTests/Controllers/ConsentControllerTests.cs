using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Authentication.Api.Models.Email;
using Authentication.IntegrationTests.Helpers;
using Authentication.IntegrationTests.Mocks;
using Authentication.IntegrationTests.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Authentication.IntegrationTests.Controllers
{
    [TestClass]
    public class ConsentControllerTests : WebHostTestBase
    {
        [TestMethod]
        public async Task Third_party_apps_show_a_consent_screen_to_the_user()
        {
            // Arrange
            var loginAttemptStatusResult = await StartLogin();

            // Act
            var returnResult = await Client.GetAsync(loginAttemptStatusResult.ReturnUrl);
            
            // Assert
            Assert.AreEqual(LoginHelpers.LoginAttemptStatus.Accepted, loginAttemptStatusResult.Status);
            Assert.AreEqual(HttpStatusCode.OK, returnResult.StatusCode);
            var requestUrl = returnResult.RequestMessage?.RequestUri?.ToString() ?? "";
            Assert.IsTrue(requestUrl.StartsWith("http://localhost/consent"), $"User was redirected to {requestUrl} instead of http://localhost/consent");
        }
        
        [TestMethod]
        public async Task Error_is_shown_when_return_url_does_not_belong_to_a_third_party_app()
        {
            // Arrange
            await StartLogin(); // user is now authenticated

            // Act
            var returnResult = await Client.GetAsync("/Consent?ReturnUrl=https://microsoft.com");
            var returnContent = await returnResult.Content.ReadAsStringAsync();
            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, returnResult.StatusCode);
            var searchFor = "Sorry, there was an error";
            Assert.IsTrue(returnContent.Contains(searchFor), $"Page should contain {searchFor}, but is:\n {returnContent}");
        }

        [TestMethod]
        public async Task If_user_gives_consent_the_third_party_app_gets_access()
        {
            // Arrange
            var loginAttemptStatusResult = await StartLogin();
            var returnResult = await Client.GetAsync(loginAttemptStatusResult.ReturnUrl);
            var consentDocument = await returnResult.GetDocumentAsync();

            // Act
            var form = consentDocument.Forms["consent"];
            var allowConsentResult = await Client.SendAsync(form, form.QuerySelector("button[value='yes']") as IHtmlElement);

            // Assert
            Assert.IsTrue(allowConsentResult.RequestMessage?.RequestUri?.ToString().StartsWith("https://localhost:5001") ?? false, $"User is forwarded to {allowConsentResult.RequestMessage?.RequestUri}, while it was expected to go to https://localhost:5001");
            Assert.IsFalse(allowConsentResult.RequestMessage.RequestUri.ToString().Contains("error=access_denied"), $"User is forwarded to {allowConsentResult.RequestMessage.RequestUri}, while the url should not contain 'error=access_denied'.");
        }

        [TestMethod]
        public async Task If_user_denies_consent_the_third_party_app_does_not_get_access()
        {
            // Arrange
            var loginAttemptStatusResult = await StartLogin();
            var returnResult = await Client.GetAsync(loginAttemptStatusResult.ReturnUrl);
            var consentDocument = await returnResult.GetDocumentAsync();

            // Act
            var form = consentDocument.Forms["consent"];
            var allowConsentResult = await Client.SendAsync(form, form.QuerySelector("button[value='no']") as IHtmlElement);

            // Assert
            Assert.IsTrue(allowConsentResult.RequestMessage?.RequestUri?.ToString().StartsWith("https://localhost:5001") ?? false, $"User is forwarded to {allowConsentResult.RequestMessage?.RequestUri}, while it was expected to go to https://localhost:5001");
            Assert.IsTrue(allowConsentResult.RequestMessage.RequestUri.ToString().Contains("error=access_denied"), $"User is forwarded to {allowConsentResult.RequestMessage.RequestUri}, while the url should contain 'error=access_denied'.");
        }
        
        [TestMethod]
        public async Task Error_is_shown_when_return_url_is_no_longer_valid_when_giving_consent()
        {
            // Arrange
            var loginAttemptStatusResult = await StartLogin();
            var returnResult = await Client.GetAsync(loginAttemptStatusResult.ReturnUrl);
            var consentDocument = await returnResult.GetDocumentAsync();

            // Act
            var form = consentDocument.Forms["consent"];
            var allowConsentResult = await Client.SendAsync(form, form.QuerySelector("button[value='yes']") as IHtmlElement, new KeyValuePair<string, string>[]
            {
                new ("ReturnUrl", "https://microsoft.com")
            });
            var allowConsentContent = await allowConsentResult.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, allowConsentResult.StatusCode);
            var searchFor = "Sorry, there was an error";
            Assert.IsTrue(allowConsentContent.Contains(searchFor), $"Page should contain {searchFor}, but is:\n {allowConsentContent}");
        }

        private async Task<LoginHelpers.LoginAttemptStatusResult> StartLogin()
        {
            var loginAttemptDocument = await Client.StartLoginAttemptToDocs("test@test.com");
            var confirmationMail = MailServiceMock.MailsSent[0].Model as NewUserEmailModel;
            await Client.ApproveLogin(confirmationMail?.ConfirmUrl);
            return await Client.CheckLogin(loginAttemptDocument);
        }
    }
}
