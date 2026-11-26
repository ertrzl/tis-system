using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tis.Domain.Entities;

namespace Tis.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.StudentNumber).HasMaxLength(20);

        // E-poçt hər universitetdə ayrı-ayrı unikaldır.
        builder.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();

        // Tələbə nömrəsi də hər universitetdə unikaldır. Boş (null) olanlar bir-biri ilə toqquşmasın deyə
        // yalnız dolu nömrələr yoxlanılır (filtrli unikal indeks).
        builder.HasIndex(u => new { u.TenantId, u.StudentNumber })
            .IsUnique()
            .HasFilter("[StudentNumber] IS NOT NULL");
    }
}
