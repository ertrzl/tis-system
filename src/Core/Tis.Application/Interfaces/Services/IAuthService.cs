using Tis.Application.DTOs.Auth;

namespace Tis.Application.Interfaces.Services;

public interface IAuthService
{
    Task<GetTokenDto> LoginAsync(PostLoginDto dto, CancellationToken ct = default);

    // Köhnə yenilənmə tokeni ilə yeni giriş + yenilənmə tokeni alır (köhnəsi ləğv olunur).
    Task<GetTokenDto> RefreshAsync(PostRefreshDto dto, CancellationToken ct = default);

    // Yenilənmə tokenini ləğv edir. Token yoxdursa və ya artıq ləğv olunubsa xəta vermir.
    Task LogoutAsync(PostRefreshDto dto, CancellationToken ct = default);

    Task<GetCurrentUserDto> GetCurrentUserAsync(CancellationToken ct = default);

    // Dəvət bağlantısı açılanda: etibarlıdırsa kimə aid olduğunu göstərir.
    Task<GetInviteDto> GetInviteAsync(string token, CancellationToken ct = default);

    // Dəvətdəki parolu təyin edir və istifadəçini dərhal daxil edir. Dəvət birdəfəlikdir.
    Task<GetTokenDto> AcceptInviteAsync(PostAcceptInviteDto dto, CancellationToken ct = default);
}
