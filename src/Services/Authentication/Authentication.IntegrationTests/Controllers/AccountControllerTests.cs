using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Authentication.Api.Models.Email;
using Authentication.Api.Quickstart.Account;
using Authentication.IntegrationTests.Helpers;
using Authentication.IntegrationTests.Mocks;
using Authentication.IntegrationTests.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Authentication.IntegrationTests.Controllers
{
    [TestClass]
    public class AccountControllerTests : WebHostTestBase
    {
        [TestMethod]
        public async Task Welcome_mail_is_sent_if_new_user_tries_to_login()
        {
            // Act
            await StartLoginAttempt("test@test.com");

            // Assert
            Assert.AreEqual(1, MailServiceMock.MailsSent.Count);
            Assert.AreEqual("test@test.com", MailServiceMock.MailsSent[0].To);
            Assert.IsInstanceOfType(MailServiceMock.MailsSent[0].Model, typeof(NewUserEmailModel));
        }

        [TestMethod]
        public async Task Login_mail_is_sent_if_existing_user_tries_to_login()
        {
            // Act
            await StartLoginAttempt("AliceSmith@email.com");

            // Assert
            Assert.AreEqual(1, MailServiceMock.MailsSent.Count);
            Assert.AreEqual("AliceSmith@email.com", MailServiceMock.MailsSent[0].To);
            Assert.IsInstanceOfType(MailServiceMock.MailsSent[0].Model, typeof(LoginEmailModel));
        }

        [TestMethod]
        public async Task Timer_is_shown_if_user_tries_to_login()
        {
            // Act
            var loginAttemptDocument = await StartLoginAttempt("test@test.com");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, loginAttemptDocument.StatusCode);
            Assert.IsNotNull(loginAttemptDocument.GetElementById("timeLeft"));
        }

        [TestMethod]
        public async Task Login_hangs_while_attempt_is_not_approved()
        {
            // Arrange
            var loginAttemptDocument = await StartLoginAttempt("test@test.com");

            // Act
            var checkResult = await CheckLogin(loginAttemptDocument);

            // Assert
            Assert.AreEqual(LoginAttemptStatus.Pending, checkResult.Status);
        }

        [TestMethod]
        public async Task Login_continues_after_attempt_is_approved()
        {
            // Arrange
            var loginAttemptDocument = await StartLoginAttempt("test@test.com");
            var approvalMail = MailServiceMock.MailsSent[0].Model as NewUserEmailModel;

            // Act
            await ApproveLogin(approvalMail?.ConfirmUrl);
            var checkResult = await CheckLogin(loginAttemptDocument);
            var returnResult = await Client.GetAsync(checkResult.ReturnUrl);

            // Assert
            Assert.AreEqual(LoginAttemptStatus.Accepted, checkResult.Status);
            Assert.IsTrue(returnResult.RequestMessage?.RequestUri?.ToString().StartsWith("http://localhost:3000") ?? false, $"User is forwarded to {returnResult.RequestMessage?.RequestUri}, while it was expected to go to http://localhost:3000");
            Assert.IsTrue(!returnResult.RequestMessage?.RequestUri?.ToString().Contains("error=access_denied") ?? false, $"User is forwarded to {returnResult.RequestMessage?.RequestUri}, while the url should contain 'error=access_denied'.");
        }

        [TestMethod]
        public async Task User_returns_with_error_to_game_after_attempt_is_rejected()
        {
            // Arrange
            var loginAttemptDocument = await StartLoginAttempt("test@test.com");
            var approvalMail = MailServiceMock.MailsSent[0].Model as NewUserEmailModel;

            // Act
            await RejectLogin(approvalMail?.ConfirmUrl);
            var checkResult = await CheckLogin(loginAttemptDocument);
            var returnResult = await Client.GetAsync(checkResult.ReturnUrl);

            // Assert
            Assert.AreEqual(LoginAttemptStatus.ExpiredOrRejected, checkResult.Status);
            Assert.IsTrue(returnResult.RequestMessage?.RequestUri?.ToString().StartsWith("http://localhost:3000") ?? false, $"User is forwarded to {returnResult.RequestMessage?.RequestUri}, while it was expected to go to http://localhost:3000");
            Assert.IsTrue(returnResult.RequestMessage.RequestUri.ToString().Contains("error=access_denied"), $"User is forwarded to {returnResult.RequestMessage.RequestUri}, while the url should contain 'error=access_denied'.");
        }

        private Task ApproveLogin(string confirmUrl)
        {
            return OpenLinkInAuthenticationMail(confirmUrl, "yes");
        }

        private Task RejectLogin(string confirmUrl)
        {
            return OpenLinkInAuthenticationMail(confirmUrl, "no");
        }

        private async Task OpenLinkInAuthenticationMail(string confirmUrl, string button)
        {
            var approvalResponse = await Client.GetAsync(confirmUrl);
            var approvalPage = await approvalResponse.GetDocumentAsync();

            await Client.SendAsync(
                (IHtmlFormElement)approvalPage.QuerySelector("form[id='confirmLogin']"),
                (IHtmlButtonElement)approvalPage.QuerySelector("button[value='" + button + "']"));
        }

        private async Task<LoginAttemptStatusResult> CheckLogin(IHtmlDocument loginAttemptDocument)
        {
            var loginAttemptId = loginAttemptDocument.GetInputFieldValue(nameof(LoginAttemptViewModel.Id));
            var returnUrl = loginAttemptDocument.GetInputFieldValue(nameof(LoginAttemptViewModel.ReturnUrl));
            var rememberLogin = loginAttemptDocument.GetInputFieldValue(nameof(LoginAttemptViewModel.RememberLogin));

            var checkResult = await Client.PostAsync("/Account/CheckLoginApproval", new Dictionary<string, string>
            {
                {nameof(LoginAttemptInputModel.Id), loginAttemptId},
                {nameof(LoginAttemptInputModel.ReturnUrl), returnUrl},
                {nameof(LoginAttemptInputModel.RememberLogin), rememberLogin}
            });

            var checkResultContent = JObject.Parse(await checkResult.Content.ReadAsStringAsync());
            var returnUrlFromResponse = checkResultContent["returnUrl"]?.Value<string>();

            if (checkResultContent["expired"]?.Value<bool>() == true)
                return new LoginAttemptStatusResult
                { Status = LoginAttemptStatus.ExpiredOrRejected, ReturnUrl = returnUrlFromResponse };

            if (checkResultContent["approved"]?.Value<bool>() == true)
                return new LoginAttemptStatusResult
                { Status = LoginAttemptStatus.Accepted, ReturnUrl = returnUrlFromResponse };

            return new LoginAttemptStatusResult { Status = LoginAttemptStatus.Pending };
        }

        public record LoginAttemptStatusResult
        {
            public LoginAttemptStatus Status { get; init; }
            public string ReturnUrl { get; init; }
        }

        public enum LoginAttemptStatus
        {
            Pending,
            ExpiredOrRejected,
            Accepted
        }

        private async Task<IHtmlDocument> StartLoginAttempt(string email)
        {
            var initUrl = "/connect/authorize?client_id=game" +
                      "&redirect_uri=http%3A%2F%2Flocalhost:3000%2Fauth%2Fsignin-callback" +
                      "&response_type=code" +
                      "&scope=openid%20profile%20characterapi" +
                      "&state=bdbb7be6a4de42e6858fa1eda338f97b" +
                      "&code_challenge=12-sp-8M_TUlBzp2DcpPsmzzib3ZimtVqpSuDF19eyU" +
                      "&code_challenge_method=S256" +
                      "&response_mode=query";
            var result = await Client.GetAsync(initUrl);
            
            var content = await result.GetDocumentAsync();

            var loginAttempt = await Client.SendAsync(
                (IHtmlFormElement)content.QuerySelector("form[id='login']"),
                (IHtmlButtonElement)content.QuerySelector("button[value='login']"),
                new[] { new KeyValuePair<string, string>(nameof(LoginInputModel.Email), email) });
            return await loginAttempt.GetDocumentAsync();
        }
    }
}
