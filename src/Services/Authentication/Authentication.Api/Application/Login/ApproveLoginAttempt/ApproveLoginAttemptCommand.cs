using System;
using MediatR;

namespace Authentication.Api.Application.Login.ApproveLoginAttempt
{
    public class ApproveLoginAttemptCommand : IRequest<LoginAttemptDto>
    {
        public Guid LoginAttemptId { get; }

        public string Secret { get; }

        public ApproveLoginAttemptCommand(Guid loginAttemptId, string secret)
        {
            LoginAttemptId = loginAttemptId;
            Secret = secret;
        }
    }
}
