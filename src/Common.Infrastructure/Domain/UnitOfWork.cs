using System.Threading;
using System.Threading.Tasks;
using Common.Domain.SeedWork;
using Common.Infrastructure.Processing;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Domain
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private readonly IDomainEventsDispatcher _domainEventsDispatcher;

        public UnitOfWork(
            DbContext context,
            IDomainEventsDispatcher domainEventsDispatcher)
        {
            _context = context;
            _domainEventsDispatcher = domainEventsDispatcher;
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            await _domainEventsDispatcher.DispatchEventsAsync(cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}