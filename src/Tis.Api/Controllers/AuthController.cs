using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tis.Application.DTOs.Auth;
using Tis.Application.Interfaces;

namespace Tis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // Giriş: universitet kodu + e-poçt + parol. Uğurlu olarsa JWT qaytarır.
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<GetTokenDto>> Login(PostLoginDto dto, CancellationToken ct)
        => Ok(await authService.LoginAsync(dto, ct));

    // Token sahibinin məlumatları. Token olmadan 401 qaytarır.
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<GetCurrentUserDto>> Me(CancellationToken ct)
        => Ok(await authService.GetCurrentUserAsync(ct));
}
