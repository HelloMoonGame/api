using System;
using CharacterApi.Domain.CharacterLocations.Rules;
using CharacterApi.Domain.SeedWork;

namespace CharacterApi.Domain.CharacterLocations
{
    public class CharacterLocation : Entity
    {
        public Guid CharacterId { get; }
        public int X { get; private set; }
        public int Y { get; private set; }
        
        private CharacterLocation(Guid characterId, int x, int y)
        {
            CharacterId = characterId;
            X = x;
            Y = y;
            
            AddDomainEvent(new CharacterMovedEvent(CharacterId, 0, 0, X, Y));
        }

        public static CharacterLocation Create(Guid characterId, int x, int y, ISingleLocationPerCharacterChecker singleLocationPerCharacterChecker)
        {
            CheckRule(new CharacterCanOnlyHaveOneLocation(singleLocationPerCharacterChecker, characterId));

            return new CharacterLocation(characterId, x, y);
        }
        
        public void ChangeLocation(int x, int y)
        {
            var domainEvent = new CharacterMovedEvent(CharacterId, X, Y, x, y);
            
            X = x;
            Y = y;
            
            AddDomainEvent(domainEvent);
        }
    }
}
