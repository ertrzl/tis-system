using Tis.Application.DTOs.Tenants;

namespace Tis.Application.Interfaces.Services;

// Universitetlərin idarəsi. Yalnız SuperAdmin (platformun sahibi) istifadə edə bilər.
public interface ITenantService
{
    Task<List<GetTenantDto>> GetAllAsync(CancellationToken ct = default);

    // Yeni universitet + ilk admin yaradır və adminə dəvət göndərir.
    Task<GetTenantDto> CreateAsync(PostTenantDto dto, CancellationToken ct = default);

    // Universiteti aktiv/passiv edir. Passiv universitetin istifadəçiləri daxil ola bilmir.
    Task<GetTenantDto> SetStatusAsync(int id, PutTenantStatusDto dto, CancellationToken ct = default);
}
