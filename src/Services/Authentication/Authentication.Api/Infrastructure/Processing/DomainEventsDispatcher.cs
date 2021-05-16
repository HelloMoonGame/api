using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Data;
using Authentication.Api.Domain.SeedWork;
using MediatR;

namespace Authentication.Api.Infrastructure.Processing
{
    public class DomainEventsDispatcher : IDomainEventsDispatcher
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;

        public DomainEventsDispatcher(IMediator mediator, ApplicationDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        public async Task DispatchEventsAsync(CancellationToken cancellationToken = default)
        {
            var domainEntities = _context.ChangeTracker
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