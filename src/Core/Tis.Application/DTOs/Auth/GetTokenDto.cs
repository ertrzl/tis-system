namespace Tis.Application.DTOs.Auth;

public class GetTokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public GetCurrentUserDto User { get; set; } = new();
}
