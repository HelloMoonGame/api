using System;
using Authentication.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Api
{
    public static class SeedData
    {
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            MigrateDatabase(context);
        }
        private static void MigrateDatabase(ApplicationDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Database dependency could not be resolved");
            context.Database.Migrate();
        }
    }
}
