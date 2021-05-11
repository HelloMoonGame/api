using System;
using System.Threading.Tasks;

namespace CharacterApi.Domain.CharacterLocations
{
    public interface ISingleLocationPerCharacterChecker
    {
        Task<bool> CharacterHasNoLocationAsync(Guid characterId);
    }
}
