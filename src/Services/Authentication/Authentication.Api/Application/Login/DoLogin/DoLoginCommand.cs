using System;
using MediatR;

namespace Authentication.Api.Application.Login.DoLogin
{
    public class DoLoginCommand : IRequest<LoginAttemptStatus>
    {
        public Guid LoginAttemptId { get; }
        
        public bool RememberLogin { get; }
        
        public string ClientId { get; }

        public DoLoginCommand(Guid loginAttemptId, bool rememberLogin, string clientId)
        {
            LoginAttemptId = loginAttemptId;
            RememberLogin = rememberLogin;
            ClientId = clientId;
        }
    }
}
