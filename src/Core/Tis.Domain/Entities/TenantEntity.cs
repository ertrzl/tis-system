using Tis.Domain.Common;

namespace Tis.Domain.Entities;

// Çoxuniversitetli (multi-tenant) dəstək: qeydlər TenantId ilə ayrılır.
public abstract class TenantEntity : BaseEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
