using System;
using Authentication.Api.Domain.Login;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Authentication.UnitTests.LoginAttempts
{
    [TestClass]
    public class CreateLoginAttemptTests
    {
        [TestMethod]
        public void Each_login_attempt_gets_a_unique_id_and_secret()
        {
            var userId = Guid.NewGuid();
            var loginAttempt1 = new LoginAttempt(userId, TimeSpan.FromHours(1));
            var loginAttempt2 = new LoginAttempt(userId, TimeSpan.FromHours(1));
            
            Assert.AreEqual(loginAttempt1.UserId, loginAttempt2.UserId);
            Assert.AreNotEqual(loginAttempt1.Id, loginAttempt2.Id);
            Assert.AreNotEqual(loginAttempt1.Secret, loginAttempt2.Secret);
        }
    }
}
