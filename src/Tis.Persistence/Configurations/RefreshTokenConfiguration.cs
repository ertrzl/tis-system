using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tis.Domain.Entities;

namespace Tis.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // SHA-256 hash-i 64 hex simvoldur. Hash-ə görə axtarılır, ona görə unikal indeks.
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(64);
        builder.Property(t => t.CreatedByIp).HasMaxLength(45);
        builder.Property(t => t.UserAgent).HasMaxLength(300);

        builder.HasIndex(t => t.TokenHash).IsUnique();
        // Bir istifadəçinin bütün tokenlərini tez tapmaq üçün (təkrar istifadə aşkarlananda hamısı ləğv olunur).
        builder.HasIndex(t => t.UserId);
    }
}
