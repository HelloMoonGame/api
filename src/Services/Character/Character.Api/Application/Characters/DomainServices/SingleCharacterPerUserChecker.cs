using System;
using System.Threading.Tasks;
using CharacterApi.Domain.Characters;

namespace CharacterApi.Application.Characters.DomainServices
{
    public class SingleCharacterPerUserChecker : ISingleCharacterPerUserChecker
    {
        private readonly ICharacterRepository _characterRepository;

        public SingleCharacterPerUserChecker(ICharacterRepository characterRepository)
        {
            _characterRepository = characterRepository;
        }

        public async Task<bool> UserHasNoCharacterAsync(Guid userId)
        {
            return await _characterRepository.GetByUserIdAsync(userId) == null;
        }
    }
}