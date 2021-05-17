using System;
using Common.Domain.SeedWork;
using Common.Domain.SharedKernel;

namespace Authentication.Api.Domain.Login
{
    public class LoginAttempt : Entity
    {
        public Guid Id { get; }
        
        public Guid UserId { get; }
        
        public string Secret { get; }
        
        public bool Accepted { get; private set; }
        
        public DateTime ExpiryDate { get; }

        private LoginAttempt() { }

        private LoginAttempt(Guid userId, TimeSpan duration)
        {
            Id = Guid.NewGuid();
            Secret = Guid.NewGuid().ToString();
            UserId = userId;
            ExpiryDate = DateTime.UtcNow.Add(duration);

            AddDomainEvent(new LoginAttemptCreatedEvent(Id, UserId, Secret));
        }
        
        public static LoginAttempt Create(Guid userId, TimeSpan duration)
        {
            return new LoginAttempt(userId, duration);
        }

        public bool Approve()
        {
            if (Accepted || ExpiryDate <= SystemClock.Now) 
                return false;
            
            Accepted = true;
            AddDomainEvent(new LoginAttemptApprovedEvent(UserId));
            return true;
        }
    }
}
