using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Tis.Application.Interfaces;
using Tis.Domain.Entities;

namespace Tis.Persistence.Contexts;

public class TisDbContext : DbContext
{
    private readonly ICurrentUser _currentUser;

    public TisDbContext(DbContextOptions<TisDbContext> options, ICurrentUser currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Department> Departments => Set<Department>();

    // Sorğuları süzmək üçün hazırkı istifadəçinin universiteti. Giriş edilməyibsə 0 olur və heç bir qeyd görünmür.
    private int CurrentTenantId => _currentUser.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>().HasIndex(t => t.Code).IsUnique();

        // E-poçt hər universitetdə ayrı-ayrı unikaldır
        modelBuilder.Entity<User>().HasIndex(u => new { u.TenantId, u.Email }).IsUnique();

        modelBuilder.Entity<Department>()
            .HasOne(d => d.Faculty)
            .WithMany(f => f.Departments)
            .HasForeignKey(d => d.FacultyId);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(TenantEntity).IsAssignableFrom(t.ClrType)))
        {
            // Universitet (Tenant) silinəndə qeydlər zəncirvari silinmir (Restrict). Həm təhlükəsizdir, həm də SQL Server-də
            // "multiple cascade paths" xətasının qarşısını alır (Tenant → Faculty → Department və Tenant → Department).
            modelBuilder.Entity(entityType.ClrType)
                .HasOne(typeof(Tenant), nameof(TenantEntity.Tenant))
                .WithMany()
                .HasForeignKey(nameof(TenantEntity.TenantId))
                .OnDelete(DeleteBehavior.Restrict);

            // Çoxuniversitetli təhlükəsizlik: hər sorğu avtomatik olaraq yalnız öz universitetinin qeydlərini görür.
            typeof(TisDbContext)
                .GetMethod(nameof(SetTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, new object[] { modelBuilder });
        }
    }

    private void SetTenantFilter<T>(ModelBuilder modelBuilder) where T : TenantEntity =>
        modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == CurrentTenantId);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetTenantOnNewEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetTenantOnNewEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // Yeni qeyd əlavə edəndə TenantId-ni əl ilə yazmağa ehtiyac yoxdur: hazırkı istifadəçinin universiteti yazılır.
    private void SetTenantOnNewEntities()
    {
        if (CurrentTenantId == 0)
            return;

        foreach (var entry in ChangeTracker.Entries<TenantEntity>().Where(e => e.State == EntityState.Added))
        {
            if (entry.Entity.TenantId == 0)
                entry.Entity.TenantId = CurrentTenantId;
        }
    }
}
