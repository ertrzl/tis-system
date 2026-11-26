using Tis.Domain.Common;

namespace Tis.Domain.Entities;

// Platformu kullanan kurum (universitet)
public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // kısa kod, örn: "BDU"
    public bool IsActive { get; set; } = true;
}
