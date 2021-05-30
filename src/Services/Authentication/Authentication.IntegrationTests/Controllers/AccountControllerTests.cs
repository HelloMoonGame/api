using System.Linq;
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
        public async Task User_is_redirected_back_to_application_when_cancelling_login()
        {
            // Arrange
            var loginPage = await Client.GetLoginPage("game", "http://localhost:3000/auth/signin-callback");
            
            // Act
            var loginAttempt = await Client.SendAsync(
                (IHtmlFormElement)loginPage.QuerySelector("form[id='login']"),
                (IHtmlButtonElement)loginPage.QuerySelector("button[value='cancel']"));
            
            // Assert
            var url = loginAttempt.RequestMessage?.RequestUri?.ToString();
            var key = "error=access_denied";
            Assert.IsNotNull(url);
            Assert.IsTrue(url.Contains(key), "{0} should contain {1}", url, key);
        }

        [TestMethod]
        public async Task Error_is_shown_if_user_does_not_fill_in_mail_address_at_login()
        {
            // Act
            var document = await Client.StartLoginAttemptToGame(null);

            // Assert
            Assert.IsNotNull(document.Forms["login"]);
            Assert.IsNotNull(document.GetElementById("Email"));
            CollectionAssert.Contains(document.GetElementById("Email").ClassList.ToList(), "input-validation-error");
        }
        
        [TestMethod]
        public async Task Error_is_shown_if_user_fills_in_invalid_mail_address_at_login()
        {
            // Act
            var document = await Client.StartLoginAttemptToGame("test");

            // Assert
            Assert.IsNotNull(document.Forms["login"]);
            Assert.IsNotNull(document.GetElementById("Email"));
            CollectionAssert.Contains(document.GetElementById("Email").ClassList.ToList(), "input-validation-error");
        }

        [TestMethod]
        public async Task No_mail_is_send_if_user_fills_in_no_mail_address()
        {
            // Act
            await Client.StartLoginAttemptToGame(null);

            // Assert
            Assert.AreEqual(0, MailServiceMock.MailsSent.Count);
        }

        [TestMethod]
        public async Task No_mail_is_send_if_user_fills_in_invalid_mail_address()
        {
            // Act
            await Client.StartLoginAttemptToGame("test");

            // Assert
            Assert.AreEqual(0, MailServiceMock.MailsSent.Count);
        }

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
        public async Task Login_mail_is_sent_if_user_logins_after_successful_login()
        {
            // Arrange
            await LoginUserToGame("test@test.com");
            await LogoutUser();
            
            // Act
            await Client.StartLoginAttemptToGame("test@test.com");

            // Assert
            Assert.AreEqual(2, MailServiceMock.MailsSent.Count);
            Assert.AreEqual("test@test.com", MailServiceMock.MailsSent[1].To);
            Assert.IsInstanceOfType(MailServiceMock.MailsSent[0].Model, typeof(NewUserEmailModel));
            Assert.IsInstanceOfType(MailServiceMock.MailsSent[1].Model, typeof(LoginEmailModel));
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
            Assert.AreEqual(HttpStatusCode.NotFound, returnResult.StatusCode);
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
            var loggedOutPage = await LogoutUser();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, loggedOutPage.StatusCode);
            Assert.IsNotNull(loggedOutPage.GetElementsByClassName("logged-out-page"));
        }

        private async Task<IHtmlDocument> LogoutUser()
        {
            var logoutResponse = await Client.GetAsync("/Account/Logout");
            var logoutPage = await logoutResponse.GetDocumentAsync();
            var loggedOutResponse = await Client.SendAsync(logoutPage.Forms["logout"],
                (IHtmlElement) logoutPage.Forms["logout"].QuerySelector("button"));
            var loggedOutPage = await loggedOutResponse.GetDocumentAsync();
            return loggedOutPage;
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
