using System;
using System.Threading;
using System.Threading.Tasks;

namespace CharacterApi.Domain.Characters
{
    public interface ICharacterRepository
    {
        Task<Character> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task<Character> GetByUserIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task AddAsync(Character character, CancellationToken cancellationToken = default);
    }
}