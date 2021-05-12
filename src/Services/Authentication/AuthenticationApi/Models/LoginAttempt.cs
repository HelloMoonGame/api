using System;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.Models
{
    public class LoginAttempt
    {
        public LoginAttempt()
        {
            Id = Guid.NewGuid().ToString();
            Secret = Guid.NewGuid().ToString();
        }
        
        public LoginAttempt(string userId, TimeSpan duration) : this()
        {
            UserId = userId;
            ExpiryDate = DateTime.UtcNow.Add(duration);
        }

        public virtual string Id { get; set; }
        
        [Required]
        public virtual string UserId { get; set; }

        [Required]
        public virtual string Secret { get; set; }
        
        public virtual bool Accepted { get; set; }
        
        public virtual DateTime ExpiryDate { get; set; }
    }
}
