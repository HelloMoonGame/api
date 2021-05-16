using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Data;
using Authentication.Api.Domain.Login;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Api.Infrastructure.Domain.Login
{
    public class LoginAttemptRepository : ILoginAttemptRepository
    {
        private readonly ApplicationDbContext _context;

        public LoginAttemptRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public async Task<LoginAttempt> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.LoginAttempts.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<LoginAttempt>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.LoginAttempts.Where(la => la.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task AddAsync(LoginAttempt loginAttempt, CancellationToken cancellationToken = default)
        {
            await _context.LoginAttempts.AddAsync(loginAttempt, cancellationToken);
        }

        public void Delete(LoginAttempt loginAttempt)
        {
            _context.LoginAttempts.Remove(loginAttempt);
        }
    }
}
