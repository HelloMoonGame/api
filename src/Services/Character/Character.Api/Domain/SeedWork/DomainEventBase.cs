using System;
using Character.Api.Domain.SharedKernel;

namespace Character.Api.Domain.SeedWork
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