using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tis.Application.DTOs.Tenants;
using Tis.Application.Interfaces.Services;
using Tis.Domain.Enums;

namespace Tis.Api.Controllers;

// Universitetlərin idarəsi: yalnız platformanın sahibi (SuperAdmin).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.SuperAdmin))]
public class TenantsController(ITenantService tenantService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GetTenantDto>>> GetAll(CancellationToken ct)
        => Ok(await tenantService.GetAllAsync(ct));

    // Yeni universitet + ilk admin yaradır, adminə parol təyin etmə dəvəti göndərir.
    [HttpPost]
    public async Task<ActionResult<GetTenantDto>> Create(PostTenantDto dto, CancellationToken ct)
        => Ok(await tenantService.CreateAsync(dto, ct));

    // Universiteti aktiv/passiv edir.
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<GetTenantDto>> SetStatus(int id, PutTenantStatusDto dto, CancellationToken ct)
        => Ok(await tenantService.SetStatusAsync(id, dto, ct));
}
