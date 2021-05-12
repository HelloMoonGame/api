using System;

namespace Authentication.Api.Quickstart.Account
{
    public class LoginAttemptViewModel : LoginAttemptInputModel
    {
        public DateTime ExpiryDate { get; set; }
    }
}
