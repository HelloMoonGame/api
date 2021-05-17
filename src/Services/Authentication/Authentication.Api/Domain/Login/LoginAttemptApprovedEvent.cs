using System;
using Common.Domain.SeedWork;

namespace Authentication.Api.Domain.Login
{
    public class LoginAttemptApprovedEvent : DomainEventBase
    {
        public Guid UserId { get; }

        public LoginAttemptApprovedEvent(Guid userId)
        {
            UserId = userId;
        }
    }
}
