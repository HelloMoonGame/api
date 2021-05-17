using System;
using MediatR;

namespace Authentication.Api.Application.Login.GetLoginAttempt
{
    public class GetLoginAttemptQuery : IRequest<LoginAttemptDto>
    {
        public Guid LoginAttemptId { get; }

        public GetLoginAttemptQuery(Guid loginAttemptId)
        {
            LoginAttemptId = loginAttemptId;
        }
    }
}
