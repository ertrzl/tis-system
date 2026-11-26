using Tis.Domain.Common;

namespace Tis.Domain.Entities;

// Dəvət: administrator istifadəçini əlavə edəndə ona e-poçtla birdəfəlik bağlantı gedir, istifadəçi parolunu orada təyin edir.
// Bazada tokenin özü yox, SHA-256 hash-i saxlanılır (yenilənmə tokeni kimi).
//
// Bilərəkdən TenantEntity deyil: bağlantıya basan şəxs hələ giriş etməyib, universitet süzgəci tətbiq oluna bilməz.
public class UserInvitation : BaseEntity
{
    public int UserId { get; set; }
    public int TenantId { get; set; }

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    // Dolu olarsa dəvət artıq istifadə olunub (parol təyin edilib).
    public DateTime? UsedAt { get; set; }

    public bool IsUsable(DateTime now) => UsedAt == null && ExpiresAt > now;
}
