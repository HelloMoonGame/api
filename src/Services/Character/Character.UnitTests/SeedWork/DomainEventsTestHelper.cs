using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Domain.SeedWork;

namespace Character.UnitTests.SeedWork
{
    public static class DomainEventsTestHelper
    {
        public static List<IDomainEvent> GetAllDomainEvents(Entity aggregate)
        {
            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            return GetEntities(aggregate)
                .SelectMany(entity => entity.DomainEvents)
                .ToList();
        }

        public static void ClearAllDomainEvents(Entity aggregate)
        {
            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            GetEntities(aggregate)
                .ToList()
                .ForEach(entity => entity.ClearDomainEvents());
        }

        private static IEnumerable<Entity> GetEntities(Entity aggregate)
        {
            var entities = new List<Entity> { aggregate };
            var fields = GetFields(aggregate);
            foreach (var field in fields)
            {
                var isEntity = field.FieldType.IsAssignableFrom(typeof(Entity));

                if (isEntity)
                {
                    var entity = field.GetValue(aggregate) as Entity;
                    entities.Add(entity);
                }

                if (field.FieldType != typeof(string) && 
                    typeof(IEnumerable).IsAssignableFrom(field.FieldType) && 
                    field.GetValue(aggregate) is IEnumerable enumerable)
                {
                    entities.AddRange(enumerable.OfType<Entity>().SelectMany(GetEntities));
                }
            }

            return entities;
        }

        private static IEnumerable<FieldInfo> GetFields(Entity aggregate)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
            var fields = aggregate.GetType()
                .GetFields(flags);

            var baseType = aggregate.GetType().BaseType;
            if (baseType != null)
                fields = fields.Concat(baseType.GetFields(flags)).ToArray();
            return fields;
        }
    }
}