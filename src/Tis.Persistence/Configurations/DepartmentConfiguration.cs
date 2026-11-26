using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tis.Domain.Entities;

namespace Tis.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);

        builder.HasOne(d => d.Faculty)
            .WithMany(f => f.Departments)
            .HasForeignKey(d => d.FacultyId);
    }
}
