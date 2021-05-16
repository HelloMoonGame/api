using System;
using System.ComponentModel.DataAnnotations;
using Common.Domain.SeedWork;

namespace Authentication.Api.Domain.Login
{
    public class LoginAttempt : Entity
    {
        public Guid Id { get; set; }
        
        public Guid UserId { get; set; }

        [Required]
        public string Secret { get; set; }
        
        public bool Accepted { get; set; }
        
        public DateTime ExpiryDate { get; set; }

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
    }
}
