using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tis.Application.DTOs.Auth;
using Tis.Application.Exceptions;
using Tis.Application.Interfaces.Services;
using Tis.Domain.Entities;
using Tis.Persistence.Contexts;

namespace Tis.Persistence.Implementations.Services;

public class AuthService(
    TisDbContext db,
    ITokenService tokenService,
    ICurrentUser currentUser,
    IClientInfo clientInfo,
    IMapper mapper,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly PasswordHasher<User> _hasher = new();

    // Ardıcıl neçə uğursuz parol cəhdindən sonra hesab bağlanır və nə qədər müddətə ("Auth" ayarları).
    private int MaxFailedAttempts => configuration.GetValue("Auth:MaxFailedAttempts", 5);
    private int LockoutMinutes => configuration.GetValue("Auth:LockoutMinutes", 15);

    public async Task<GetTokenDto> LoginAsync(PostLoginDto dto, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Kod, nömrə/e-poçt və ya parol səhv olsa eyni mesaj qaytarılır ki, universitetin
        // və ya istifadəçinin mövcudluğu bilinməsin.
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Code == dto.TenantCode && t.IsActive, ct)
                     ?? throw new UnauthorizedException(Loc.T("auth.invalidCredentials"));

        var identifier = dto.Identifier.Trim();

        // Giriş zamanı hələ token yoxdur, ona görə tenant süzgəci söndürülür və TenantId əl ilə yazılır.
        // "@" varsa e-poçt, yoxdursa tələbə nömrəsi kimi axtarılır.
        var users = db.Users.IgnoreQueryFilters().Where(u => u.TenantId == tenant.Id);
        var user = identifier.Contains('@')
            ? await users.FirstOrDefaultAsync(u => u.Email == identifier.ToLowerInvariant(), ct)
            : await users.FirstOrDefaultAsync(u => u.StudentNumber == identifier.ToUpperInvariant(), ct);

        if (user is null)
            throw new UnauthorizedException(Loc.T("auth.invalidCredentials"));

        // Hesab bağlıdırsa parola baxılmır belə: doğru parol da müddət bitənə qədər qəbul edilmir.
        if (user.LockoutEnd is { } lockoutEnd && lockoutEnd > now)
        {
            var minutesLeft = (int)Math.Ceiling((lockoutEnd - now).TotalMinutes);
            throw new TooManyRequestsException(Loc.T("auth.accountLocked", minutesLeft));
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedAttempts)
            {
                user.LockoutEnd = now.AddMinutes(LockoutMinutes);
                user.FailedLoginCount = 0;
                logger.LogWarning("User {UserId} locked for {Minutes} minutes after too many failed logins", user.Id, LockoutMinutes);
            }

            await db.SaveChangesAsync(ct);
            throw new UnauthorizedException(Loc.T("auth.invalidCredentials"));
        }

        // Uğurlu giriş: sayğac və kilid sıfırlanır.
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;

        return await IssueTokensAsync(user, tenant.Name, now, replacing: null, ct);
    }

    public async Task<GetTokenDto> RefreshAsync(PostRefreshDto dto, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var hash = tokenService.HashRefreshToken(dto.RefreshToken);

        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
                     ?? throw new UnauthorizedException(Loc.T("auth.invalidRefresh"));

        if (stored.RevokedAt != null)
        {
            // İstifadə olunmuş token yenidən gəldi: ya oğurlanıb, ya da kimsə köhnə surətlə işləyir.
            // Təhlükəsizlik üçün bu istifadəçinin bütün sessiyaları bağlanır, yenidən daxil olmalıdır.
            await db.RefreshTokens
                .Where(t => t.UserId == stored.UserId && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);

            logger.LogWarning("A used refresh token was presented again for user {UserId}: all their sessions were revoked", stored.UserId);
            throw new UnauthorizedException(Loc.T("auth.invalidRefresh"));
        }

        if (stored.ExpiresAt <= now)
            throw new UnauthorizedException(Loc.T("auth.invalidRefresh"));

        var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == stored.UserId, ct)
                   ?? throw new UnauthorizedException(Loc.T("auth.invalidRefresh"));

        // Universitet bu arada deaktiv edilibsə sessiya davam etmir.
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == user.TenantId && t.IsActive, ct)
                     ?? throw new UnauthorizedException(Loc.T("auth.invalidRefresh"));

        return await IssueTokensAsync(user, tenant.Name, now, replacing: stored, ct);
    }

    public async Task LogoutAsync(PostRefreshDto dto, CancellationToken ct = default)
    {
        var hash = tokenService.HashRefreshToken(dto.RefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null, ct);

        if (stored is null)
            return; // artıq ləğv olunub və ya yoxdur: çıxış üçün bu da uğurdur

        stored.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<GetCurrentUserDto> GetCurrentUserAsync(CancellationToken ct = default)
    {
        // Burada tenant süzgəci işləyir: yalnız öz universitetinin istifadəçisi tapıla bilər.
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId, ct)
                   ?? throw new NotFoundException(Loc.T("common.notFound"));

        var tenant = await db.Tenants.FirstAsync(t => t.Id == user.TenantId, ct);
        return ToDto(user, tenant.Name);
    }

    // Giriş + yenilənmə tokeni yaradır və yenilənmə tokenini saxlayır. Yenilənmə zamanı köhnə token eyni saxlamada ləğv olunur.
    private async Task<GetTokenDto> IssueTokensAsync(User user, string tenantName, DateTime now, RefreshToken? replacing, CancellationToken ct)
    {
        var (accessToken, accessExpiresAt) = tokenService.CreateAccessToken(user);
        var (refreshToken, refreshHash, refreshExpiresAt) = tokenService.CreateRefreshToken();

        if (replacing is not null)
        {
            replacing.RevokedAt = now;
            replacing.ReplacedByTokenHash = refreshHash;
        }

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            TokenHash = refreshHash,
            ExpiresAt = refreshExpiresAt,
            CreatedByIp = clientInfo.IpAddress,
            UserAgent = clientInfo.UserAgent
        });

        await db.SaveChangesAsync(ct);

        return new GetTokenDto
        {
            AccessToken = accessToken,
            ExpiresAt = accessExpiresAt,
            RefreshToken = refreshToken,
            RefreshExpiresAt = refreshExpiresAt,
            User = ToDto(user, tenantName)
        };
    }

    private GetCurrentUserDto ToDto(User user, string tenantName)
    {
        var dto = mapper.Map<GetCurrentUserDto>(user);
        dto.TenantName = tenantName;
        return dto;
    }
}
