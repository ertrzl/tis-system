using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tis.Domain.Entities;

namespace Tis.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(20);

        // Universitet kodu bütün sistemdə unikaldır (giriş zamanı universiteti bununla tapırıq).
        builder.HasIndex(t => t.Code).IsUnique();
    }
}
