namespace Tis.Application.DTOs.Auth;

public class PostLoginDto
{
    public string TenantCode { get; set; } = string.Empty;  // universitetin qısa kodu, məs: "BDU"
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
