using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Tis.Application.Interfaces.Services;
using Tis.Domain.Common;
using Tis.Domain.Entities;
using Tis.Persistence.Converters;

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
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Sorğuları süzmək üçün hazırkı istifadəçinin universiteti. Giriş edilməyibsə 0 olur və heç bir qeyd görünmür.
    private int CurrentTenantId => _currentUser.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Hər cədvəlin Fluent API qaydaları öz IEntityTypeConfiguration<T> sinfindədir (Persistence/Configurations).
        // Bu sətir onların hamısını avtomatik tapır.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TisDbContext).Assembly);

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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Bütün tarix sahələri bazadan UTC kimi oxunur (bax: UtcDateTimeConverter).
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    private void SetTenantFilter<T>(ModelBuilder modelBuilder) where T : TenantEntity =>
        modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == CurrentTenantId);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAutomaticFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAutomaticFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // Saxlamazdan əvvəl avtomatik doldurulan sahələr:
    //  - yeni qeyddə TenantId (hazırkı istifadəçinin universiteti), əl ilə yazmağa ehtiyac yoxdur;
    //  - dəyişən qeyddə UpdatedAt.
    private void ApplyAutomaticFields()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added && entry.Entity is TenantEntity tenantEntity
                && tenantEntity.TenantId == 0 && CurrentTenantId != 0)
                tenantEntity.TenantId = CurrentTenantId;
        }
    }
}
