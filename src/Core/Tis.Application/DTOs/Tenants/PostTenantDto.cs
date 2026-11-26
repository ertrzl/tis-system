namespace Tis.Application.DTOs.Tenants;

// SuperAdmin yeni universitet yaradır və onun ilk adminini təyin edir. Adminə parol təyin etmək üçün dəvət gedir.
public class PostTenantDto
{
    public string Code { get; set; } = string.Empty;        // qısa kod, məs: "BDU" (girişdə istifadə olunur)
    public string Name { get; set; } = string.Empty;        // məs: "Bakı Dövlət Universiteti"
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminFirstName { get; set; } = string.Empty;
    public string AdminLastName { get; set; } = string.Empty;
}
