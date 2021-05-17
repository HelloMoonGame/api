using System;
using Authentication.Api.Domain.Login;
using Common.Application.Configuration.DomainEvents;
using Newtonsoft.Json;

namespace Authentication.Api.Application.Login.IntegrationHandlers
{
    public class LoginAttemptStartedNotification  : DomainNotificationBase<LoginAttemptCreatedEvent>
    {
        public Guid LoginAttemptId { get; }

        public Guid UserId { get; }

        public string Secret { get; }

        public LoginAttemptStartedNotification(LoginAttemptCreatedEvent domainEvent) : base(domainEvent)
        {
            LoginAttemptId = domainEvent.LoginAttemptId;
            UserId = domainEvent.UserId;
            Secret = domainEvent.Secret;
        }

        [JsonConstructor]
        public LoginAttemptStartedNotification(Guid loginAttemptId, Guid userId, string secret) : base(null)
        {
            LoginAttemptId = loginAttemptId;
            UserId = userId;
            Secret = secret;
        }
    }
}
