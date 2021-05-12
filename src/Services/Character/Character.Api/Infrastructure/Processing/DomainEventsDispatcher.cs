using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CharacterApi.Domain.SeedWork;
using CharacterApi.Infrastructure.Database;
using MediatR;

namespace CharacterApi.Infrastructure.Processing
{
    public class DomainEventsDispatcher : IDomainEventsDispatcher
    {
        private readonly IMediator _mediator;
        private readonly CharactersContext _charactersContext;

        public DomainEventsDispatcher(IMediator mediator, CharactersContext charactersContext)
        {
            _mediator = mediator;
            _charactersContext = charactersContext;
        }

        public async Task DispatchEventsAsync(CancellationToken cancellationToken = default)
        {
            var domainEntities = _charactersContext.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any()).ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();
            
            domainEntities
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            var tasks = domainEvents
                .Select(async (domainEvent) =>
                {
                    await _mediator.Publish(domainEvent, cancellationToken);
                });

            await Task.WhenAll(tasks);
        }
    }
}