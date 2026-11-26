using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tis.Api.Extensions;
using Tis.Application.DTOs.Auth;
using Tis.Application.Interfaces.Services;

namespace Tis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // Giriş: universitet kodu + e-poçt + parol. Uğurlu olarsa JWT qaytarır.
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    public async Task<ActionResult<GetTokenDto>> Login(PostLoginDto dto, CancellationToken ct)
        => Ok(await authService.LoginAsync(dto, ct));

    // Köhnə yenilənmə tokeni ilə yeni token cütü alır (köhnəsi ləğv olunur).
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    public async Task<ActionResult<GetTokenDto>> Refresh(PostRefreshDto dto, CancellationToken ct)
        => Ok(await authService.RefreshAsync(dto, ct));

    // Çıxış: yenilənmə tokenini ləğv edir. Giriş tokeni bitmiş ola bilər, ona görə yenilənmə tokeni özü etibarnamədir.
    [HttpPost("logout")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    public async Task<IActionResult> Logout(PostRefreshDto dto, CancellationToken ct)
    {
        await authService.LogoutAsync(dto, ct);
        return NoContent();
    }

    // Token sahibinin məlumatları. Token olmadan 401 qaytarır.
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<GetCurrentUserDto>> Me(CancellationToken ct)
        => Ok(await authService.GetCurrentUserAsync(ct));
}
