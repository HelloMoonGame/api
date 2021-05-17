using System;

namespace Authentication.Api.InputModels
{
    public class LoginAttemptInputModel
    {
        public Guid Id { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberLogin { get; set; }
    }
}