namespace AuthenticationApi.Models.Email
{
    public class NewUserEmailModel : EmailModel
    {
        public override string ViewName => "NewUser";
        public override string Subject => "Welcome to Hello Moon!";
        
        public string Email { get; set; }
        public string ConfirmUrl { get; set; }
    }
}
