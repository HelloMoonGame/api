using System;
using System.Threading.Tasks;

namespace Character.Api.Domain.Characters
{
    public interface ISingleCharacterPerUserChecker
    {
        Task<bool> UserHasNoCharacterAsync(Guid userId);
    }
}
