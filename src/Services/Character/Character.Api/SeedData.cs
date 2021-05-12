using System;
using Character.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Character.Api
{
    public static class SeedData
    {
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var context = scope.ServiceProvider.GetService<CharactersContext>();
            MigrateDatabase(context);
        }

        private static void MigrateDatabase(CharactersContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Database dependency could not be resolved");
            context.Database.Migrate();
        }
    }
}
