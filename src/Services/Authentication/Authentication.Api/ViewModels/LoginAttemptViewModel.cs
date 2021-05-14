using System;
using Authentication.Api.InputModels;

namespace Authentication.Api.ViewModels
{
    public class LoginAttemptViewModel : LoginAttemptInputModel
    {
        public DateTime ExpiryDate { get; set; }
    }
}
