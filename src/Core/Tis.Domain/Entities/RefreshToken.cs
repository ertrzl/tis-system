using Tis.Domain.Common;

namespace Tis.Domain.Entities;

// Yenilənmə tokeni: qısa ömürlü giriş tokeni bitəndə istifadəçini yenidən parol yazdırmadan yenisini almaq üçün.
// Bazada tokenin özü yox, yalnız SHA-256 hash-i saxlanılır (baza oğurlansa belə token istifadə edilə bilməz).
//
// Bilərəkdən TenantEntity deyil: yeniləmə zamanı istifadəçi hələ tanınmır (giriş tokeni bitib), ona görə
// universitet süzgəci tətbiq oluna bilməz. TenantId yenə də saxlanılır.
public class RefreshToken : BaseEntity
{
    // Bilərəkdən "User" naviqasiyası və xarici açar yoxdur: User-də universitet süzgəci var, yeniləmə zamanı isə istifadəçi
    // hələ tanınmır. Köhnəlmiş tokenləri MaintenanceBackgroundService silir.
    public int UserId { get; set; }

    public int TenantId { get; set; }

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    // Dolu olarsa token artıq istifadə olunub (yenilənib) və ya çıxışla ləğv edilib.
    public DateTime? RevokedAt { get; set; }

    // Yenilənəndə əvəzində verilən yeni tokenin hash-i (zəncirin izi).
    public string? ReplacedByTokenHash { get; set; }

    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }

    public bool IsActive(DateTime now) => RevokedAt == null && ExpiresAt > now;
}
