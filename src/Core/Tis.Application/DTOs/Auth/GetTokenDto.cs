namespace Tis.Application.DTOs.Auth;

public class GetTokenDto
{
    // Qısa ömürlü giriş tokeni (hər sorğuda göndərilir)
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    // Uzun ömürlü yenilənmə tokeni (yalnız yeni giriş tokeni almaq üçün). Hər istifadədə yenisi ilə əvəz olunur.
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshExpiresAt { get; set; }

    public GetCurrentUserDto User { get; set; } = new();
}
