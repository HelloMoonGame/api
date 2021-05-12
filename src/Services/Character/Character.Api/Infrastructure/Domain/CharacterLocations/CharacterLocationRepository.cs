using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Character.Api.Domain.CharacterLocations;
using Character.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Character.Api.Infrastructure.Domain.CharacterLocations
{
    public class CharacterLocationRepository : ICharacterLocationRepository
    {
        private readonly CharactersContext _context;

        public CharacterLocationRepository(CharactersContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(CharacterLocation characterLocation, CancellationToken cancellationToken = default)
        {
            await _context.CharacterLocations.AddAsync(characterLocation, cancellationToken);
        }

        public async Task<CharacterLocation> GetByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken = default)
        {
            return await _context.CharacterLocations.SingleOrDefaultAsync(c => c.CharacterId == characterId, cancellationToken);
        }

        public async Task<IEnumerable<CharacterLocation>> GetByCharacterIdsAsync(IEnumerable<Guid> characterIds, CancellationToken cancellationToken = default)
        {
            return await _context.CharacterLocations.Where(c => characterIds.Contains(c.CharacterId)).ToListAsync(cancellationToken);
        }
    }
}