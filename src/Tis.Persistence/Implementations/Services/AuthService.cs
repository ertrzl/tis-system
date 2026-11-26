using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tis.Application.DTOs.Auth;
using Tis.Application.Exceptions;
using Tis.Application.Interfaces;
using Tis.Domain.Entities;
using Tis.Persistence.Contexts;

namespace Tis.Persistence.Implementations.Services;

public class AuthService(TisDbContext db, ITokenService tokenService, ICurrentUser currentUser) : IAuthService
{
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<GetTokenDto> LoginAsync(PostLoginDto dto, CancellationToken ct = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        // Hansı universitetə girdiyini bilmirik: universitet kodundan tapırıq. Kod, e-poçt və ya parol səhv olsa
        // eyni mesaj qaytarılır ki, universitetin və ya istifadəçinin mövcudluğu bilinməsin.
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Code == dto.TenantCode && t.IsActive, ct)
                     ?? throw new UnauthorizedException(Loc.T("auth.invalidCredentials"));

        // Giriş zamanı hələ token yoxdur, ona görə tenant süzgəci söndürülür və TenantId əl ilə yazılır.
        var user = await db.Users.IgnoreQueryFilters()
                       .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Email == email, ct)
                   ?? throw new UnauthorizedException(Loc.T("auth.invalidCredentials"));

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException(Loc.T("auth.invalidCredentials"));

        var (token, expiresAt) = tokenService.CreateToken(user);

        return new GetTokenDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            User = ToDto(user, tenant.Name)
        };
    }

    public async Task<GetCurrentUserDto> GetCurrentUserAsync(CancellationToken ct = default)
    {
        // Burada tenant süzgəci işləyir: yalnız öz universitetinin istifadəçisi tapıla bilər.
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId, ct)
                   ?? throw new NotFoundException(Loc.T("common.notFound"));

        var tenant = await db.Tenants.FirstAsync(t => t.Id == user.TenantId, ct);
        return ToDto(user, tenant.Name);
    }

    private static GetCurrentUserDto ToDto(User user, string tenantName) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role.ToString(),
        TenantId = user.TenantId,
        TenantName = tenantName
    };
}
