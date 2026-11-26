namespace Tis.Domain.Entities;

// Her universitet için çoklu kurum (multi-tenant) desteği: kayıtlar TenantId ile ayrılır.
public abstract class TenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
