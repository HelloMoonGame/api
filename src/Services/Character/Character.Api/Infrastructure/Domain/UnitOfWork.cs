using System.Threading;
using System.Threading.Tasks;
using Character.Api.Domain.SeedWork;
using Character.Api.Infrastructure.Database;
using Character.Api.Infrastructure.Processing;

namespace Character.Api.Infrastructure.Domain
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CharactersContext _charactersContext;
        private readonly IDomainEventsDispatcher _domainEventsDispatcher;

        public UnitOfWork(
            CharactersContext charactersContext,
            IDomainEventsDispatcher domainEventsDispatcher)
        {
            _charactersContext = charactersContext;
            _domainEventsDispatcher = domainEventsDispatcher;
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            await _domainEventsDispatcher.DispatchEventsAsync(cancellationToken);
            return await _charactersContext.SaveChangesAsync(cancellationToken);
        }
    }
}