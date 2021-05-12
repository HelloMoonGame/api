namespace Authentication.Api.Quickstart.Account
{
    public class LoginAttemptConfirmViewModel : LoginAttemptConfirmInputModel
    {
        public bool ExpiredOrNonExisting { get; set; }
        public bool WasAlreadyConfirmed { get; set; }
        public bool Accepted { get; set; }
    }
}
