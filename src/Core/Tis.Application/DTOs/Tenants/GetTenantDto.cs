namespace Tis.Application.DTOs.Tenants;

public class GetTenantDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPlatform { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Yalnız yaradılma cavabında doludur: ilk adminə dəvət e-poçtunun göndərilib-göndərilmədiyi.
    public bool? InviteEmailSent { get; set; }
}
