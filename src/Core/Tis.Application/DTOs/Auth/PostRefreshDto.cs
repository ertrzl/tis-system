namespace Tis.Application.DTOs.Auth;

// Həm yeniləmə (refresh), həm də çıxış (logout) üçün istifadə olunur.
public class PostRefreshDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
