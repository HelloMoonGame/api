using System;
using CharacterApi.Domain.SeedWork;

namespace CharacterApi.Domain.Characters
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