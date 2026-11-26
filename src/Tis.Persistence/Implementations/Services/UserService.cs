using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Tis.Application.Common;
using Tis.Application.DTOs.Users;
using Tis.Application.Exceptions;
using Tis.Application.Interfaces.Services;
using Tis.Domain.Enums;
using Tis.Persistence.Contexts;

namespace Tis.Persistence.Implementations.Services;

public class UserService(TisDbContext db, ICurrentUser currentUser, IMapper mapper) : IUserService
{
    public async Task<PagedResult<GetUserDto>> GetPagedAsync(UserQuery query, CancellationToken ct = default)
    {
        if (currentUser.Role != nameof(UserRole.Admin))
            throw new ForbiddenException(Loc.T("auth.forbidden"));

        // db.Users universitet süzgəci ilə gəlir: admin yalnız öz universitetinin istifadəçilərini görür.
        IQueryable<Domain.Entities.User> users = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            users = users.Where(u =>
                u.Email.Contains(search)
                || u.FirstName.Contains(search)
                || u.LastName.Contains(search)
                || (u.StudentNumber != null && u.StudentNumber.Contains(search)));
        }

        var total = await users.CountAsync(ct);

        var items = await users
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ThenBy(u => u.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<GetUserDto>
        {
            Items = mapper.Map<List<GetUserDto>>(items),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }
}
