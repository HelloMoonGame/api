using System;
using Common.Domain.SeedWork;

namespace Authentication.Api.Domain.Login
{
    public class LoginAttemptCreatedEvent : DomainEventBase
    {
        public Guid LoginAttemptId { get; }
        
        public Guid UserId { get; }

        public string Secret { get; }

        public LoginAttemptCreatedEvent(Guid loginAttemptId, Guid userId, string secret)
        {
            LoginAttemptId = loginAttemptId;
            UserId = userId;
            Secret = secret;
        }
    }
}