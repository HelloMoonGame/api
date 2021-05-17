using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Authentication.Api.Domain.Login
{
    public interface ILoginAttemptRepository
    {
        Task<LoginAttempt> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<LoginAttempt>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(LoginAttempt loginAttempt, CancellationToken cancellationToken = default);
        void Delete(LoginAttempt loginAttempt);
    }
}
