using System;
using System.Threading.Tasks;
using CharacterApi.Domain.CharacterLocations;

namespace CharacterApi.Application.CharacterLocations.DomainServices
{
    public class SingleLocationPerCharacterChecker : ISingleLocationPerCharacterChecker
    {
        private readonly ICharacterLocationRepository _characterLocationRepository;

        public SingleLocationPerCharacterChecker(ICharacterLocationRepository characterLocationRepository)
        {
            _characterLocationRepository = characterLocationRepository;
        }

        public async Task<bool> CharacterHasNoLocationAsync(Guid characterId)
        {
            return await _characterLocationRepository.GetByCharacterIdAsync(characterId) == null;
        }
    }
}