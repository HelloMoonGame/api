using System;

namespace IdentityServerHost.Quickstart.UI
{
    public class LoginAttemptViewModel : LoginAttemptInputModel
    {
        public DateTime ExpiryDate { get; set; }
    }
}
