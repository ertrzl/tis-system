using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Tis.Application.DTOs.Tenants;
using Tis.Application.Exceptions;
using Tis.Application.Interfaces.Services;
using Tis.Domain.Entities;
using Tis.Domain.Enums;
using Tis.Persistence.Contexts;

namespace Tis.Persistence.Implementations.Services;

public class TenantService(
    TisDbContext db,
    ICurrentUser currentUser,
    IInvitationService invitations,
    IMapper mapper) : ITenantService
{
    public async Task<List<GetTenantDto>> GetAllAsync(CancellationToken ct = default)
    {
        EnsureSuperAdmin();

        var tenants = await db.Tenants.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);

        // İstifadəçi sayları: universitet süzgəci söndürülür, çünki SuperAdmin bütün universitetləri sayır.
        var counts = await db.Users.IgnoreQueryFilters()
            .GroupBy(u => u.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TenantId, x => x.Count, ct);

        return tenants.Select(t => ToDto(t, counts.GetValueOrDefault(t.Id))).ToList();
    }

    public async Task<GetTenantDto> CreateAsync(PostTenantDto dto, CancellationToken ct = default)
    {
        EnsureSuperAdmin();

        var code = dto.Code.Trim().ToUpperInvariant();
        if (await db.Tenants.AnyAsync(t => t.Code == code, ct))
            throw new ConflictException(Loc.T("tenant.codeExists"));

        // Universitet və onun ilk admini bir addımda yaranır: biri alınmasa digəri də qalmır.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var tenant = new Tenant { Code = code, Name = dto.Name.Trim() };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);

        // TenantId açıq yazılır: hazırkı istifadəçi (SuperAdmin) başqa universitetə aiddir, avtomatik doldurma işləməz.
        // Boş PasswordHash = dəvət gözlənilir; parolu admin dəvət bağlantısında özü təyin edir.
        var admin = new User
        {
            TenantId = tenant.Id,
            Email = dto.AdminEmail.Trim().ToLowerInvariant(),
            FirstName = dto.AdminFirstName.Trim(),
            LastName = dto.AdminLastName.Trim(),
            Role = UserRole.Admin,
            PasswordHash = string.Empty
        };
        db.Users.Add(admin);
        await db.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        // E-poçt tranzaksiyadan sonra göndərilir: göndərmə uğursuz olsa belə universitet yaradılıb.
        var emailSent = await invitations.CreateAndSendAsync(admin, tenant.Name, ct);

        var result = ToDto(tenant, userCount: 1);
        result.InviteEmailSent = emailSent;
        return result;
    }

    public async Task<GetTenantDto> SetStatusAsync(int id, PutTenantStatusDto dto, CancellationToken ct = default)
    {
        EnsureSuperAdmin();

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct)
                     ?? throw new NotFoundException(Loc.T("tenant.notFound"));

        if (tenant.Code == Tenant.PlatformCode && !dto.IsActive)
            throw new BadRequestException(Loc.T("tenant.cannotDisablePlatform"));

        tenant.IsActive = dto.IsActive;
        await db.SaveChangesAsync(ct);

        var userCount = await db.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == tenant.Id, ct);
        return ToDto(tenant, userCount);
    }

    // Controller-dəki [Authorize(Roles)] ilə yanaşı ikinci qat: servis başqa yerdən çağırılsa belə yoxlama qalır.
    private void EnsureSuperAdmin()
    {
        if (currentUser.Role != nameof(UserRole.SuperAdmin))
            throw new ForbiddenException(Loc.T("auth.forbidden"));
    }

    private GetTenantDto ToDto(Tenant tenant, int userCount)
    {
        var dto = mapper.Map<GetTenantDto>(tenant);
        dto.UserCount = userCount;
        return dto;
    }
}
