using System;
using Authentication.Api.Domain.Login;
using Common.Domain.SharedKernel;

namespace Authentication.Api.Application.Login
{
    public class LoginAttemptDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Secret { get; set; }
        public DateTime ExpiryDate { get; set; }
        private bool _accepted;

        public LoginAttemptStatus Status
        {
            get
            {
                if (ExpiryDate <= SystemClock.Now)
                    return LoginAttemptStatus.Expired;
                if (_accepted)
                    return LoginAttemptStatus.Approved;

                return LoginAttemptStatus.Pending;
            }
        }
        
        public static LoginAttemptDto FromLoginAttempt(LoginAttempt loginAttempt)
        {
            return new()
            {
                Id = loginAttempt.Id,
                UserId = loginAttempt.UserId,
                Secret = loginAttempt.Secret,
                _accepted = loginAttempt.Accepted,
                ExpiryDate = loginAttempt.ExpiryDate
            };
        }
    }
}
