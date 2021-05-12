using System;
using System.Threading.Tasks;

namespace Character.Api.Domain.CharacterLocations
{
    public interface ISingleLocationPerCharacterChecker
    {
        Task<bool> CharacterHasNoLocationAsync(Guid characterId);
    }
}
