namespace Tis.Domain.Entities;

// Kafedra
public class Department : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public int FacultyId { get; set; }
    public Faculty? Faculty { get; set; }
}
