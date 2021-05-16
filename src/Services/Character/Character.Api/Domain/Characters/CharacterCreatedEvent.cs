using System;
using Common.Domain.SeedWork;

namespace Character.Api.Domain.Characters
{
    public class CharacterCreatedEvent : DomainEventBase
    {
        public Guid CharacterId { get; }

        public CharacterCreatedEvent(Guid characterId)
        {
            CharacterId = characterId;
        }
    }
}