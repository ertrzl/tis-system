namespace Tis.Application.Interfaces.Services;

// Hazırkı sorğunu göndərən istifadəçi (JWT token-dan oxunur).
// TenantId verilənlər bazası sorğularını həmin universitetə görə süzmək üçün istifadə olunur.
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    int UserId { get; }
    int TenantId { get; }   // giriş edilməyibsə 0
    string? Role { get; }
}
