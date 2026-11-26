using Tis.Application.DTOs.Auth;

namespace Tis.Application.Interfaces;

public interface IAuthService
{
    Task<GetTokenDto> LoginAsync(PostLoginDto dto, CancellationToken ct = default);
    Task<GetCurrentUserDto> GetCurrentUserAsync(CancellationToken ct = default);
}
