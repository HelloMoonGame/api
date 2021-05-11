using CharacterApi.Domain.CharacterLocations;
using CharacterApi.Domain.Characters;
using Microsoft.EntityFrameworkCore;

namespace CharacterApi.Infrastructure.Database
{
    public class CharactersContext : DbContext
    {
        public DbSet<Character> Characters { get; set; }
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