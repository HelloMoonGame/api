using System;
using MediatR;

namespace Authentication.Api.Application.Login.RejectLoginAttempt
{
    public class RejectLoginAttemptCommand : IRequest<LoginAttemptDto>
    {
        public Guid LoginAttemptId { get; }
        
        public string Secret { get; set; }

        public RejectLoginAttemptCommand(Guid loginAttemptId, string secret)
        {
            LoginAttemptId = loginAttemptId;
            Secret = secret;
        }
    }
}
