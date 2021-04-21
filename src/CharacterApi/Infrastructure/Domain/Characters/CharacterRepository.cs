using System;
using System.Threading;
using System.Threading.Tasks;
using CharacterApi.Domain.Characters;
using CharacterApi.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CharacterApi.Infrastructure.Domain.Characters
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly CharactersContext _context;

        public CharacterRepository(CharactersContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Character character, CancellationToken cancellationToken = default)
        {
            await _context.Characters.AddAsync(character, cancellationToken);
        }

        public async Task<Character> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Characters.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Character> GetByUserIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Characters.SingleOrDefaultAsync(c => c.UserId == id, cancellationToken);
        }
    }
}