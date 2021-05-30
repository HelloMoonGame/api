using System;
using System.Collections.Generic;
using System.Linq;
using Common.Application.Configuration.DomainEvents;
using Common.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Processing
{
    public class DomainNotificationFactory
    {
        private readonly ILogger<DomainNotificationFactory> _logger;
        private readonly IEnumerable<Type> _notificationTypes;
        
        public DomainNotificationFactory(ILogger<DomainNotificationFactory> logger, params Type[] assemblyMarkerTypes)
        {
            _logger = logger;
            
            var typesInAssembly = assemblyMarkerTypes
                .SelectMany(rootType => rootType.Assembly.GetTypes())
                .ToList();
            _logger.LogDebug("Found {Amount} types in assemblies", typesInAssembly.Count);

            var accessibleTypes = typesInAssembly
                .Where(type => !type.IsAbstract && !type.IsInterface && type.IsPublic)
                .ToList();
            _logger.LogDebug("Found {Amount} accessible types in assemblies", accessibleTypes.Count);

            _notificationTypes = accessibleTypes
                .Where(type => typeof(IDomainEventNotification<IDomainEvent>).IsAssignableFrom(type))
                .Distinct()
                .ToList();
            _logger.LogDebug("Found {Amount} notification types in assemblies", _notificationTypes.Count());
        }

        public IDomainEventNotification<T> CreateNotification<T>(T domainEvent) where T : IDomainEvent
        {
            var genericType = typeof(IDomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notificationType = _notificationTypes
                .SingleOrDefault(type => genericType.IsAssignableFrom(type));
            
            if (notificationType == null)
                return null;
            
            _logger.LogDebug("Found notificationType {NotificationType} for {DomainEvent}", notificationType.ToString(), domainEvent.GetType().ToString());
            
            return Activator.CreateInstance(notificationType, domainEvent) as IDomainEventNotification<T>;
        }

        public Type GetTypeByFullName(string typeName)
        {
            return _notificationTypes.Single(type => type.FullName == typeName);
        }
    }
}
