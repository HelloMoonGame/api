using System;
using Common.Domain.SeedWork;

namespace Character.Api.Domain.CharacterLocations.Rules
{
    public class CharacterCanOnlyHaveOneLocation : IBusinessRule
    {
        private readonly ISingleLocationPerCharacterChecker _singleLocationPerCharacterChecker;

        private readonly Guid _character;

        public CharacterCanOnlyHaveOneLocation(
            ISingleLocationPerCharacterChecker singleLocationPerCharacterChecker,
            Guid character)
        {
            _singleLocationPerCharacterChecker = singleLocationPerCharacterChecker;
            _character = character;
        }

        public bool IsBroken() => !_singleLocationPerCharacterChecker.CharacterHasNoLocationAsync(_character).Result;

        public string Message => "Character cannot be on two location. Update the existing one.";
    }
}