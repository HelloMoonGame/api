using System;
using System.Collections.Generic;
using System.Linq;
using Authentication.Api;
using Authentication.Api.Application.Login.IntegrationHandlers;
using Authentication.Api.Domain.Login;
using Common.Domain.SeedWork;
using Common.Infrastructure.Processing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
            var loginAttempt1 = LoginAttempt.Create(userId, TimeSpan.FromHours(1));
            var loginAttempt2 = LoginAttempt.Create(userId, TimeSpan.FromHours(1));
            
            Assert.AreEqual(loginAttempt1.UserId, loginAttempt2.UserId);
            Assert.AreNotEqual(loginAttempt1.Id, loginAttempt2.Id);
            Assert.AreNotEqual(loginAttempt1.Secret, loginAttempt2.Secret);
        }
        
        [TestMethod]
        public void DomainNotificationFactory_should_find_LoginAttemptStartedNotification()
        {
            var domainNotificationFactory = new DomainNotificationFactory(
                NullLoggerFactory.Instance.CreateLogger<DomainNotificationFactory>(), 
                typeof(Startup));
            Assert.AreEqual(typeof(LoginAttemptStartedNotification),
                domainNotificationFactory.GetTypeByFullName(typeof(LoginAttemptStartedNotification).FullName));
        }

        [TestMethod]
        public void DomainNotificationFactory_should_create_LoginAttemptStartedNotification_from_LoginAttemptCreatedEvent()
        {
            var loginAttemptId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var domainEvent = new LoginAttemptCreatedEvent(loginAttemptId, userId, "test");
            var domainNotificationFactory = new DomainNotificationFactory(
                NullLoggerFactory.Instance.CreateLogger<DomainNotificationFactory>(),
                typeof(Startup));
            var notification = domainNotificationFactory.CreateNotification(domainEvent);
            
            Assert.IsNotNull(notification);
            Assert.IsInstanceOfType(notification, typeof(LoginAttemptStartedNotification));
            Assert.AreEqual(domainEvent, notification.DomainEvent);

            var loginAttemptStartedNotification = (LoginAttemptStartedNotification)notification;
            Assert.AreEqual(loginAttemptId, loginAttemptStartedNotification.LoginAttemptId);
            Assert.AreEqual(userId, loginAttemptStartedNotification.UserId);
            Assert.AreEqual("test", loginAttemptStartedNotification.Secret);
        }

        [TestMethod]
        public void DomainNotificationFactory_should_not_create_Notifications_for_Events_without_Notification()
        {
            var userId = Guid.NewGuid();
            var loginAttemptId = Guid.NewGuid();
            var domainEvent = new LoginAttemptCreatedEvent(loginAttemptId, userId, "test");
            var domainEvent2 = new LoginAttemptApprovedEvent(userId);
            var domainNotificationFactory = new DomainNotificationFactory(
                NullLoggerFactory.Instance.CreateLogger<DomainNotificationFactory>(),
                typeof(Startup));

            var domainEvents = new List<IDomainEvent>
            {
                domainEvent, domainEvent2
            };
            var domainEventNotifications = domainEvents
                .Select(domainNotificationFactory.CreateNotification)
                .Where(notification => notification != null)
                .ToList();
            
            Assert.AreEqual(1, domainEventNotifications.Count);
        }
    }
}
