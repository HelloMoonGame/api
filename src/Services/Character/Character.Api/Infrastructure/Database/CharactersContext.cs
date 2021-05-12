using Character.Api.Domain.CharacterLocations;
using Microsoft.EntityFrameworkCore;

namespace Character.Api.Infrastructure.Database
{
    public class CharactersContext : DbContext
    {
        public DbSet<Api.Domain.Characters.Character> Characters { get; set; }
        public DbSet<CharacterLocation> CharacterLocations { get; set; }

        public CharactersContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CharactersContext).Assembly);
        }
    }
}