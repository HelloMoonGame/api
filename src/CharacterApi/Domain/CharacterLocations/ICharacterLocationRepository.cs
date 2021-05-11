using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CharacterApi.Domain.CharacterLocations
{
    public interface ICharacterLocationRepository
    {
        Task<CharacterLocation> GetByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<CharacterLocation>> GetByCharacterIdsAsync(IEnumerable<Guid> characterIds, CancellationToken cancellationToken = default);

        Task AddAsync(CharacterLocation characterLocation, CancellationToken cancellationToken = default);
    }
}