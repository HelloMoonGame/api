namespace IdentityServerHost.Quickstart.UI
{
    public class LoginAttemptConfirmViewModel : LoginAttemptConfirmInputModel
    {
        public bool ExpiredOrNonExisting { get; set; }
        public bool WasAlreadyConfirmed { get; set; }
    }
}
