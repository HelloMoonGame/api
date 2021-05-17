using Authentication.Api.Domain.Login;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Authentication.Api.Infrastructure.Domain.Login
{

    internal sealed class LoginAttemptEntityTypeConfiguration : IEntityTypeConfiguration<LoginAttempt>
    {
        public void Configure(EntityTypeBuilder<LoginAttempt> builder)
        {
            builder.ToTable("LoginAttempts");

            builder.HasKey(b => b.Id);

            builder.Property("UserId").HasColumnName("UserId");
            builder.Property("Secret").HasColumnName("Secret").IsRequired();
            builder.Property("Accepted").HasColumnName("Accepted");
            builder.Property("ExpiryDate").HasColumnName("ExpiryDate");
        }
    }
}