namespace Tis.Infrastructure.Services;

// appsettings.json-dakı "Jwt" bölməsi. SecretKey gizlidir: Development-də appsettings.Development.json-da,
// Docker/Production-da Jwt__SecretKey mühit dəyişənində saxlanır, repoya yazılmır.
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Tis";
    public string Audience { get; set; } = "TisClient";
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
