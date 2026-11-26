using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Tis.Application.Interfaces.Services;

namespace Tis.Infrastructure.Services;

// Hazırkı HTTP sorğusunun JWT claim-lərindən istifadəçini oxuyur.
public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int UserId => ReadInt(JwtRegisteredClaimNames.Sub);

    public int TenantId => ReadInt(TokenService.TenantClaim);

    public string? Role => Principal?.FindFirstValue(TokenService.RoleClaim);

    private int ReadInt(string claimType) =>
        int.TryParse(Principal?.FindFirstValue(claimType), out var value) ? value : 0;
}
