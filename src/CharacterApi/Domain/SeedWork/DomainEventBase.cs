using System;
using CharacterApi.Domain.SharedKernel;

namespace CharacterApi.Domain.SeedWork
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