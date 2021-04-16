namespace AuthenticationApi.Models.Email
{
    public class LoginEmailModel : EmailModel
    {
        public override string ViewName => "Login";
        public override string Subject => "Confirm your login attempt";
        
        public string Email { get; set; }
        public string ConfirmUrl { get; set; }
    }
}
