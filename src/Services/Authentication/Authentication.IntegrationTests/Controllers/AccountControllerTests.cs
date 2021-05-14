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
    public class AccountControllerTests : WebHostTestBase
    {
        [TestMethod]
        public async Task Welcome_mail_is_sent_if_new_user_tries_to_login()
        {
            // Act
            await Client.StartLoginAttemptToGame("test@test.com");

            // Assert
            Assert.AreEqual(1, MailServiceMock.MailsSent.Count);
            Assert.AreEqual("test@test.com", MailServiceMock.MailsSent[0].To);
            Assert.IsInstanceOfType(MailServiceMock.MailsSent[0].Model, typeof(NewUserEmailModel));
        }

        [TestMethod]
        public async Task Login_mail_is_sent_if_existing_user_tries_to_login()
        {
            // Act
            await Client.StartLoginAttemptToGame("AliceSmith@email.com");

            // Assert
            Assert.AreEqual(1, MailServiceMock.MailsSent.Count);
            Assert.AreEqual("AliceSmith@email.com", MailServiceMock.MailsSent[0].To);
            Assert.IsInstanceOfType(MailServiceMock.MailsSent[0].Model, typeof(LoginEmailModel));
        }

        [TestMethod]
        public async Task Timer_is_shown_if_user_tries_to_login()
        {
            // Act
            var loginAttemptDocument = await Client.StartLoginAttemptToGame("test@test.com");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, loginAttemptDocument.StatusCode);
            Assert.IsNotNull(loginAttemptDocument.GetElementById("timeLeft"));
        }

        [TestMethod]
        public async Task Login_hangs_while_attempt_is_not_approved()
        {
            // Arrange
            var loginAttemptDocument = await Client.StartLoginAttemptToGame("test@test.com");

            // Act
            var checkResult = await Client.CheckLogin(loginAttemptDocument);

            // Assert
            Assert.AreEqual(LoginHelpers.LoginAttemptStatus.Pending, checkResult.Status);
        }

        [TestMethod]
        public async Task Login_continues_after_attempt_is_approved()
        {
            // Arrange
            var loginAttemptDocument = await Client.StartLoginAttemptToGame("test@test.com");
            var approvalMail = MailServiceMock.MailsSent[0].Model as NewUserEmailModel;

            // Act
            await Client.ApproveLogin(approvalMail?.ConfirmUrl);
            var checkResult = await Client.CheckLogin(loginAttemptDocument);
            var returnResult = await Client.GetAsync(checkResult.ReturnUrl);

            // Assert
            Assert.AreEqual(LoginHelpers.LoginAttemptStatus.Accepted, checkResult.Status);
            Assert.IsTrue(returnResult.RequestMessage?.RequestUri?.ToString().StartsWith("http://localhost:3000") ?? false, $"User is forwarded to {returnResult.RequestMessage?.RequestUri}, while it was expected to go to http://localhost:3000");
            Assert.IsTrue(!returnResult.RequestMessage?.RequestUri?.ToString().Contains("error=access_denied") ?? false, $"User is forwarded to {returnResult.RequestMessage?.RequestUri}, while the url should contain 'error=access_denied'.");
        }

        [TestMethod]
        public async Task User_returns_with_error_to_game_after_attempt_is_rejected()
        {
            // Arrange
            var loginAttemptDocument = await Client.StartLoginAttemptToGame("test@test.com");
            var approvalMail = MailServiceMock.MailsSent[0].Model as NewUserEmailModel;

            // Act
            await Client.RejectLogin(approvalMail?.ConfirmUrl);
            var checkResult = await Client.CheckLogin(loginAttemptDocument);
            var returnResult = await Client.GetAsync(checkResult.ReturnUrl);

            // Assert
            Assert.AreEqual(LoginHelpers.LoginAttemptStatus.ExpiredOrRejected, checkResult.Status);
            Assert.IsTrue(returnResult.RequestMessage?.RequestUri?.ToString().StartsWith("http://localhost:3000") ?? false, $"User is forwarded to {returnResult.RequestMessage?.RequestUri}, while it was expected to go to http://localhost:3000");
            Assert.IsTrue(returnResult.RequestMessage.RequestUri.ToString().Contains("error=access_denied"), $"User is forwarded to {returnResult.RequestMessage.RequestUri}, while the url should contain 'error=access_denied'.");
        }
        
        [TestMethod]
        public async Task Unauthenticated_users_do_not_have_to_confirm_logout()
        {
            // Act
            var logoutResponse = await Client.GetAsync("/Account/Logout");
            var logoutPage = await logoutResponse.GetDocumentAsync();
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, logoutResponse.StatusCode);
            Assert.IsNull(logoutPage.Forms["logout"]);
        }

        [TestMethod]
        public async Task Authenticated_users_get_a_confirm_page_when_trying_to_logout()
        {
            // Arrange
            await LoginUserToGame("test@test.com");

            // Act
            var logoutResponse = await Client.GetAsync("/Account/Logout");
            var logoutPage = await logoutResponse.GetDocumentAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, logoutResponse.StatusCode);
            Assert.IsNotNull(logoutPage.Forms["logout"]);
        }

        [TestMethod]
        public async Task User_gets_a_confirmation_after_logout()
        {
            // Arrange
            await LoginUserToGame("test@test.com");

            // Act
            var logoutResponse = await Client.GetAsync("/Account/Logout");
            var logoutPage = await logoutResponse.GetDocumentAsync();
            var loggedOutResponse = await Client.SendAsync(logoutPage.Forms["logout"], 
                (IHtmlElement)logoutPage.Forms["logout"].QuerySelector("button"));
            var loggedOutPage = await loggedOutResponse.GetDocumentAsync();
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, loggedOutPage.StatusCode);
            Assert.IsNotNull(loggedOutPage.GetElementsByClassName("logged-out-page"));
        }

        private async Task LoginUserToGame(string email)
        {
            var loginAttemptDocument = await Client.StartLoginAttemptToGame(email);
            var confirmationMail = MailServiceMock.MailsSent[0].Model as NewUserEmailModel;
            await Client.ApproveLogin(confirmationMail?.ConfirmUrl);
            await Client.CheckLogin(loginAttemptDocument);
        }
    }
}
