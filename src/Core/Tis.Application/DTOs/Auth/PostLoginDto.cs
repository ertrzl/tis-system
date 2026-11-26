namespace Tis.Application.DTOs.Auth;

public class PostLoginDto
{
    public string TenantCode { get; set; } = string.Empty;   // universitetin qısa kodu, məs: "BDU"
    public string Identifier { get; set; } = string.Empty;   // tələbə nömrəsi VƏ YA e-poçt
    public string Password { get; set; } = string.Empty;
}
