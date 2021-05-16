using System;

namespace Authentication.Api.InputModels
{
    public class LoginAttemptConfirmInputModel
    {
        public Guid? Id { get; set; }
        public string Secret { get; set; }
    }
}
