namespace Authentication.Api.Application.Logout
{
    public class LogoutDto
    {
        public string PostLogoutRedirectUri { get; set; }
        public string ClientName { get; set; }
        public string SignOutIframeUrl { get; set; }
        public string LogoutId { get; set; }
    }
}
