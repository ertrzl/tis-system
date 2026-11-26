using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tis.Application.Interfaces.Services;
using Tis.Domain.Entities;

namespace Tis.Infrastructure.Services;

public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    // Token-un içindəki claim adları. "tid" = universitetin (tenant) id-si.
    public const string TenantClaim = "tid";
    public const string RoleClaim = "role";

    private readonly JwtOptions _jwt = options.Value;

    public (string Token, DateTime ExpiresAt) CreateAccessToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(TenantClaim, user.TenantId.ToString()),
            new Claim(RoleClaim, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public (string Token, string Hash, DateTime ExpiresAt) CreateRefreshToken()
    {
        // 64 təsadüfi bayt: təxmin edilə bilməz. URL-ə uyğun Base64 mətni kimi müştəriyə verilir.
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        return (token, HashToken(token), DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays));
    }

    public (string Token, string Hash) CreateOneTimeToken()
    {
        // 32 təsadüfi bayt (256 bit) təxmin edilə bilməz; bağlantıda qısa qalsın deyə yenilənmə tokenindən kiçikdir.
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        return (token, HashToken(token));
    }

    // Bazada tokenin özü deyil, SHA-256 hash-i saxlanılır: baza oğurlansa belə token istifadə edilə bilməz.
    public string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
