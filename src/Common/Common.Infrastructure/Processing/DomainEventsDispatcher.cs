using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Application.Configuration.DomainEvents;
using Common.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Common.Infrastructure.Processing
{
    public class DomainEventsDispatcher : IDomainEventsDispatcher
    {
        private readonly IMediator _mediator;
        private readonly DomainNotificationFactory _domainNotificationFactory;
        private readonly DbContext _context;

        public DomainEventsDispatcher(IMediator mediator, DomainNotificationFactory domainNotificationFactory, DbContext context)
        {
            _mediator = mediator;
            _domainNotificationFactory = domainNotificationFactory;
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

            var domainEventNotifications = domainEvents
                .Select(_domainNotificationFactory.CreateNotification)
                .Where(notification => notification != null)
                .ToList();

            domainEntities
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            var tasks = domainEvents
                .Select(async (domainEvent) =>
                {
                    await _mediator.Publish(domainEvent, cancellationToken);
                });

            await Task.WhenAll(tasks);

            foreach (var domainEventNotification in domainEventNotifications)
            {
                var type = domainEventNotification.GetType().FullName;
                var data = JsonConvert.SerializeObject(domainEventNotification);
                
                // These notifications should be pushed to a service bus or message queue.
                // For now, outbox notifications are handled here directly.
                var requestType = _domainNotificationFactory.GetTypeByFullName(type);
                var request = JsonConvert.DeserializeObject(data, requestType) as IDomainEventNotification;
                await _mediator.Publish(request, cancellationToken);
            }
        }
    }
}