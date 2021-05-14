using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AngleSharp.Html.Dom;
using Authentication.Api.Quickstart.Account;
using Newtonsoft.Json.Linq;

namespace Authentication.IntegrationTests.Helpers
{
    public static class LoginHelpers
    {

        public static async Task<LoginAttemptStatusResult> CheckLogin(this HttpClient client, IHtmlDocument loginAttemptDocument)
        {
            var loginAttemptId = loginAttemptDocument.GetInputFieldValue(nameof(LoginAttemptViewModel.Id));
            var returnUrl = loginAttemptDocument.GetInputFieldValue(nameof(LoginAttemptViewModel.ReturnUrl));
            var rememberLogin = loginAttemptDocument.GetInputFieldValue(nameof(LoginAttemptViewModel.RememberLogin));

            var checkResult = await client.PostAsync("/Account/CheckLoginApproval", new Dictionary<string, string>
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

        public static Task<IHtmlDocument> StartLoginAttemptToGame(this HttpClient client, string email)
        {
            return StartLoginAttempt(client, "game", "http://localhost:3000/auth/signin-callback", email);
        }

        public static Task<IHtmlDocument> StartLoginAttemptToDocs(this HttpClient client, string email)
        {
            return StartLoginAttempt(client, "docs", "https://docs.hellomoon.nl/auth/signin-callback", email);
        }

        private static async Task<IHtmlDocument> StartLoginAttempt(HttpClient client, string clientId, string redirectUri, string email)
        {
            var initUrl = "/connect/authorize?client_id=" + clientId +
                          "&redirect_uri=" + HttpUtility.UrlEncode(redirectUri) +
                          "&response_type=code" +
                          "&scope=openid%20email%20characterapi" +
                          "&state=bdbb7be6a4de42e6858fa1eda338f97b" +
                          "&code_challenge=12-sp-8M_TUlBzp2DcpPsmzzib3ZimtVqpSuDF19eyU" +
                          "&code_challenge_method=S256" +
                          "&response_mode=query";
            var result = await client.GetAsync(initUrl);

            var content = await result.GetDocumentAsync();

            var loginAttempt = await client.SendAsync(
                (IHtmlFormElement)content.QuerySelector("form[id='login']"),
                (IHtmlButtonElement)content.QuerySelector("button[value='login']"),
                new[] { new KeyValuePair<string, string>(nameof(LoginInputModel.Email), email) });
            return await loginAttempt.GetDocumentAsync();
        }

        public static Task ApproveLogin(this HttpClient client, string confirmUrl)
        {
            return OpenLinkInAuthenticationMail(client, confirmUrl, "yes");
        }

        public static Task RejectLogin(this HttpClient client, string confirmUrl)
        {
            return OpenLinkInAuthenticationMail(client, confirmUrl, "no");
        }

        private static async Task OpenLinkInAuthenticationMail(HttpClient client, string confirmUrl, string button)
        {
            var approvalResponse = await client.GetAsync(confirmUrl);
            var approvalPage = await approvalResponse.GetDocumentAsync();

            await client.SendAsync(
                approvalPage.Forms["confirmLogin"],
                (IHtmlElement)approvalPage.QuerySelector("button[value='" + button + "']"));
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
    }
}
