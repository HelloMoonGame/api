using System;
using MediatR;

namespace Authentication.Api.Application.Login.StartLoginAttempt
{
    public class StartLoginAttemptCommand : IRequest<LoginAttemptDto>
    {
        public Guid UserId { get; set; }
        
        public bool RememberLogin { get; set; }
        
        public string ReturnUrl { get; set; }

        public StartLoginAttemptCommand(Guid userId, bool rememberLogin, string returnUrl)
        {
            UserId = userId;
            RememberLogin = rememberLogin;
            ReturnUrl = returnUrl;
        }
    }
}
