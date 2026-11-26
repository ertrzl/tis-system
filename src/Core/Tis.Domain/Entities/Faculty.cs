namespace Tis.Domain.Entities;

// Fakültə
public class Faculty : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public List<Department> Departments { get; set; } = new();
}
