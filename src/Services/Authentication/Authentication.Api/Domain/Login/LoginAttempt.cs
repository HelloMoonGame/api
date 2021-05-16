using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.Domain.Login
{
    public class LoginAttempt
    {
        public LoginAttempt()
        {
            Id = Guid.NewGuid();
            Secret = Guid.NewGuid().ToString();
        }
        
        public LoginAttempt(Guid userId, TimeSpan duration) : this()
        {
            UserId = userId;
            ExpiryDate = DateTime.UtcNow.Add(duration);
        }

        public Guid Id { get; set; }
        
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Secret { get; set; }
        
        public bool Accepted { get; set; }
        
        public DateTime ExpiryDate { get; set; }
    }
}
