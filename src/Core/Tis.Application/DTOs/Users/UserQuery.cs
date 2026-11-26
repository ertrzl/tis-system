namespace Tis.Application.DTOs.Users;

// İstifadəçi siyahısının sorğu parametrləri (?page=1&pageSize=20&search=...).
public class UserQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
}
