using Tis.Application.Common;
using Tis.Application.DTOs.Users;

namespace Tis.Application.Interfaces.Services;

// Hazırkı universitetin istifadəçiləri. Universitet süzgəci DbContext-də olduğu üçün admin yalnız öz universitetini görür.
public interface IUserService
{
    Task<PagedResult<GetUserDto>> GetPagedAsync(UserQuery query, CancellationToken ct = default);
}
