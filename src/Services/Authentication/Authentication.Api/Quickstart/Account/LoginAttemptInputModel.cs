namespace Authentication.Api.Quickstart.Account
{
    public class LoginAttemptInputModel
    {
        public string Id { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberLogin { get; set; }
    }
}