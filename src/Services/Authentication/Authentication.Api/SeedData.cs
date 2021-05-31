using System;
using System.Linq;
using Authentication.Api.Data;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Api
{
    public static class SeedData
    {
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var configuration = scope.ServiceProvider.GetService<IConfiguration>();

            var applicationDbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            MigrateDatabase(applicationDbContext);
            
            var configurationDbContext = scope.ServiceProvider.GetService<ConfigurationDbContext>();
            MigrateDatabase(configurationDbContext);
            
            var persistedGrantDbContext = scope.ServiceProvider.GetService<PersistedGrantDbContext>();
            MigrateDatabase(persistedGrantDbContext);
            
            foreach (var client in Config.Clients(configuration["GameUrl"],
                configuration["CharacterApiUrl"]))
            {
                if (configurationDbContext.Clients.All(c => c.ClientId != client.ClientId))
                {
                    var corsUri = new Uri(client.RedirectUris.First());
                    client.AllowedCorsOrigins.Add(corsUri.Scheme + "://" + corsUri.Authority);
                    configurationDbContext.Clients.Add(client.ToEntity());
                }
            }
            configurationDbContext.SaveChanges();
            
            foreach (var resource in Config.IdentityResources)
            {
                if (configurationDbContext.IdentityResources.All(r => r.Name != resource.Name))
                    configurationDbContext.IdentityResources.Add(resource.ToEntity());
            }
            configurationDbContext.SaveChanges();

            foreach (var apiScope in Config.ApiScopes)
            {
                if (configurationDbContext.ApiScopes.All(s => s.Name != apiScope.Name))
                    configurationDbContext.ApiScopes.Add(apiScope.ToEntity());
            }
            configurationDbContext.SaveChanges();
        }
        private static void MigrateDatabase(DbContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Database dependency could not be resolved");
            context.Database.Migrate();
        }
    }
}
