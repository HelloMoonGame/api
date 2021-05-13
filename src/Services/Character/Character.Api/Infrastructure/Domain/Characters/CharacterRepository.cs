using System;
using System.Threading;
using System.Threading.Tasks;
using Character.Api.Domain.Characters;
using Character.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Character.Api.Infrastructure.Domain.Characters
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly CharactersContext _context;

        public CharacterRepository(CharactersContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Api.Domain.Characters.Character character, CancellationToken cancellationToken = default)
        {
            await _context.Characters.AddAsync(character, cancellationToken);
        }

        public async Task<Api.Domain.Characters.Character> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Characters.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Api.Domain.Characters.Character> GetByUserIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Characters.SingleOrDefaultAsync(c => c.UserId == id, cancellationToken);
        }
    }
}