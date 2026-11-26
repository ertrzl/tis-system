namespace Tis.Domain.Common;

// Bütün verilənlər bazası cədvəllərinin ortaq əsası. CreatedAt və UpdatedAt-ı DbContext özü doldurur,
// beləliklə hər qeydin nə vaxt yarandığı və axırıncı dəfə nə vaxt dəyişdiyi məlumdur.
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
