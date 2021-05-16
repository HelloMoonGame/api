using System;
using System.Collections.Generic;
using System.Linq;
using Common.Application.Configuration.DomainEvents;
using Common.Domain.SeedWork;

namespace Common.Infrastructure.Processing
{
    public class DomainNotificationFactory
    {
        private readonly IEnumerable<Type> _notificationTypes;
        
        public DomainNotificationFactory(params Type[] assemblyMarkerTypes)
        {
            var typesInAssembly = assemblyMarkerTypes
                .SelectMany(rootType => rootType.Assembly.GetTypes())
                .ToList();

            var accessibleTypes = typesInAssembly
                .Where(type => !type.IsAbstract && !type.IsInterface && type.IsPublic)
                .ToList();
            
            _notificationTypes = accessibleTypes
                .Where(type => typeof(IDomainEventNotification<IDomainEvent>).IsAssignableFrom(type))
                .Distinct()
                .ToList();
        }

        public IDomainEventNotification<T> CreateNotification<T>(T domainEvent) where T : IDomainEvent
        {
            var notificationType = _notificationTypes
                .SingleOrDefault(type => typeof(IDomainEventNotification<T>).IsAssignableFrom(type));
            
            if (notificationType == null)
                return null;
            
            return Activator.CreateInstance(notificationType, domainEvent) as IDomainEventNotification<T>;
        }

        public Type GetTypeByFullName(string typeName)
        {
            return _notificationTypes.Single(type => type.FullName == typeName);
        }
    }
}
