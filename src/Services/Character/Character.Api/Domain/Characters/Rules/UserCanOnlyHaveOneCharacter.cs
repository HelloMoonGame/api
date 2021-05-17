using System;
using Common.Domain.SeedWork;

namespace Character.Api.Domain.Characters.Rules
{
    public class UserCanOnlyHaveOneCharacter : IBusinessRule
    {
        private readonly ISingleCharacterPerUserChecker _singleCharacterPerUserChecker;

        private readonly Guid _userId;

        public UserCanOnlyHaveOneCharacter(
            ISingleCharacterPerUserChecker singleCharacterPerUserChecker,
            Guid userId)
        {
            _singleCharacterPerUserChecker = singleCharacterPerUserChecker;
            _userId = userId;
        }

        public bool IsBroken() => !_singleCharacterPerUserChecker.UserHasNoCharacterAsync(_userId).Result;

        public string Message => "User cannot create a second character.";
    }
}