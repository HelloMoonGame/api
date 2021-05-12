using System;
using System.Threading.Tasks;
using Character.Api.Domain.Characters;

namespace Character.Api.Application.Characters.DomainServices
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