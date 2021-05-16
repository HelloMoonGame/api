using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Data;
using Authentication.Api.Domain.SeedWork;
using Authentication.Api.Infrastructure.Processing;

namespace Authentication.Api.Infrastructure.Domain
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IDomainEventsDispatcher _domainEventsDispatcher;

        public UnitOfWork(
            ApplicationDbContext context,
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