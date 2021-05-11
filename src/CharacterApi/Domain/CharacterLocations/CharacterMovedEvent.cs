using System;
using CharacterApi.Domain.SeedWork;

namespace CharacterApi.Domain.CharacterLocations
{
    public class CharacterMovedEvent : DomainEventBase
    {
        public Guid CharacterId { get; }
        public int FromX { get; }
        public int FromY { get; }
        public int ToX { get; }
        public int ToY { get; }

        public CharacterMovedEvent(Guid characterId, int fromX, int fromY, int toX, int toY)
        {
            CharacterId = characterId;
            FromX = fromX;
            FromY = fromY;
            ToX = toX;
            ToY = toY;
        }
    }
}