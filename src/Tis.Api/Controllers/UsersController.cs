using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tis.Application.Common;
using Tis.Application.DTOs.Users;
using Tis.Application.Interfaces.Services;
using Tis.Domain.Enums;

namespace Tis.Api.Controllers;

// Universitetin istifadəçiləri: yalnız həmin universitetin admini. Universitet süzgəci DbContext-dədir,
// ona görə admin başqa universitetin istifadəçisini görə bilməz.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetUserDto>>> GetPaged([FromQuery] UserQuery query, CancellationToken ct)
        => Ok(await userService.GetPagedAsync(query, ct));
}
