using Tis.Domain.Enums;

namespace Tis.Domain.Entities;

public class User : TenantEntity
{
    public string Email { get; set; } = string.Empty;

    // Tələbə nömrəsi: universitet verir, həmin universitetdə unikaldır. Giriş üçün istifadə olunur.
    // Müəllim və adminlərdə boşdur (onlar e-poçtla daxil olur). Hər nömrə bir e-poçta bağlıdır (Email).
    public string? StudentNumber { get; set; }

    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    // Hesab kilidi: ardıcıl uğursuz parol cəhdlərini sayır; həddi aşanda LockoutEnd-ə qədər giriş bağlanır.
    public int FailedLoginCount { get; set; }
    public DateTime? LockoutEnd { get; set; }
}
