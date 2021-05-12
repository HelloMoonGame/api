using CharacterApi.Domain.CharacterLocations;
using CharacterApi.Domain.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CharacterApi.Infrastructure.Domain.CharacterLocations
{
    internal sealed class CharacterLocationEntityTypeConfiguration : IEntityTypeConfiguration<CharacterLocation>
    {
        public void Configure(EntityTypeBuilder<CharacterLocation> builder)
        {
            builder.ToTable("CharacterLocations");

            builder.HasKey(b => b.CharacterId);

            builder.Property("X").HasColumnName("X");
            builder.Property("Y").HasColumnName("Y");
        }
    }
}