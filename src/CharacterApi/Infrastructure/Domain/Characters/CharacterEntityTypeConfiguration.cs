using CharacterApi.Domain.Characters;
using CharacterApi.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CharacterApi.Infrastructure.Domain.Characters
{

    internal sealed class CharacterEntityTypeConfiguration : IEntityTypeConfiguration<Character>
    {
        public void Configure(EntityTypeBuilder<Character> builder)
        {
            builder.ToTable("Characters");

            builder.HasKey(b => b.Id);

            builder.Property("UserId").HasColumnName("UserId");
            builder.Property("FirstName").HasColumnName("FirstName");
            builder.Property("LastName").HasColumnName("LastName");
            builder.Property("Sex").HasColumnName("Sex").HasConversion(new EnumToNumberConverter<SexType, byte>());
        }
    }
}