namespace Tis.Application.DTOs.Auth;

// Dəvət bağlantısı açılanda göstərilən məlumat: "Salam X, Y universitetinə xoş gəlmisiniz".
public class GetInviteDto
{
    public string FirstName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? StudentNumber { get; set; }
    public string TenantName { get; set; } = string.Empty;
}
