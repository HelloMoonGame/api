using Character.Api.Domain.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Character.Api.Infrastructure.Domain.Characters
{

    internal sealed class CharacterEntityTypeConfiguration : IEntityTypeConfiguration<Api.Domain.Characters.Character>
    {
        public void Configure(EntityTypeBuilder<Api.Domain.Characters.Character> builder)
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