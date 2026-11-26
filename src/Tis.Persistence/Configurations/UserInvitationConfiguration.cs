using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tis.Domain.Entities;

namespace Tis.Persistence.Configurations;

public class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        // SHA-256 hash-i 64 hex simvoldur. Bağlantıdakı tokenə görə axtarılır, ona görə unikal indeks.
        builder.Property(i => i.TokenHash).IsRequired().HasMaxLength(64);
        builder.HasIndex(i => i.TokenHash).IsUnique();

        // Bir istifadəçinin köhnə dəvətlərini tez tapmaq üçün (yeni dəvət gedəndə köhnələr ləğv olunur).
        // Yenilənmə tokenində olduğu kimi User ilə xarici açar yoxdur: User-də universitet süzgəci var.
        builder.HasIndex(i => i.UserId);
    }
}
