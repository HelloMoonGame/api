using System;
using Common.Domain.SharedKernel;

namespace Common.Domain.SeedWork
{
    public class DomainEventBase : IDomainEvent
    {
        public DomainEventBase()
        {
            OccurredOn = SystemClock.Now;
        }

        public DateTime OccurredOn { get; }
    }
}