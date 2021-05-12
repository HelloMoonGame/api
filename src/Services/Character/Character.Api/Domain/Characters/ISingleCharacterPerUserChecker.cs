using System;
using System.Threading.Tasks;

namespace CharacterApi.Domain.Characters
{
    public interface ISingleCharacterPerUserChecker
    {
        Task<bool> UserHasNoCharacterAsync(Guid userId);
    }
}
