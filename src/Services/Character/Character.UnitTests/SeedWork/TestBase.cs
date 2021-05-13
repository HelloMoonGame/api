using System;
using System.Collections.Generic;
using System.Linq;
using Character.Api.Domain.SeedWork;
using Character.Api.Domain.SharedKernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Character.UnitTests.SeedWork
{
    public abstract class TestBase : IDisposable
    {
        public static T AssertPublishedDomainEvent<T>(Entity aggregate) where T : IDomainEvent
        {
            var domainEvent = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().SingleOrDefault();

            if (domainEvent == null)
            {
                throw new Exception($"{typeof(T).Name} event not published");
            }

            return domainEvent;
        }

        public static List<T> AssertPublishedDomainEvents<T>(Entity aggregate) where T : IDomainEvent
        {
            var domainEvents = DomainEventsTestHelper.GetAllDomainEvents(aggregate).OfType<T>().ToList();

            if (!domainEvents.Any())
            {
                throw new Exception($"{typeof(T).Name} event not published");
            }

            return domainEvents;
        }

        public static void AssertBrokenRule<TRule>(Action testDelegate) where TRule : class, IBusinessRule
        {
            var message = $"Expected {typeof(TRule).Name} broken rule";
            var businessRuleValidationException = Assert.ThrowsException<BusinessRuleValidationException>(testDelegate, message);
            if (businessRuleValidationException != null)
            {
                Assert.IsInstanceOfType(businessRuleValidationException.BrokenRule, typeof(TRule), message);
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            SystemClock.Reset();
        }
    }
}
