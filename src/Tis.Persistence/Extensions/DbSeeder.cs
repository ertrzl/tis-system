using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tis.Domain.Entities;
using Tis.Domain.Enums;
using Tis.Persistence.Contexts;

namespace Tis.Persistence.Extensions;

public static class DbSeeder
{
    // İlk universiteti və onun adminini yaradır. Yalnız "Seed:AdminPassword" təyin olunubsa işləyir:
    // parol heç vaxt koda yazılmır, appsettings.Development.json və ya mühit dəyişənindən (Seed__AdminPassword) gəlir.
    public static async Task SeedInitialDataAsync(this WebApplication app)
    {
        var password = app.Configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
            return;

        var code = app.Configuration["Seed:TenantCode"] ?? "DEMO";
        var tenantName = app.Configuration["Seed:TenantName"] ?? "Demo Universiteti";
        var email = (app.Configuration["Seed:AdminEmail"] ?? "admin@demo.az").Trim().ToLowerInvariant();

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TisDbContext>();

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Code == code);
        if (tenant is null)
        {
            tenant = new Tenant { Code = code, Name = tenantName };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        var exists = await db.Users.IgnoreQueryFilters().AnyAsync(u => u.TenantId == tenant.Id && u.Email == email);
        if (exists)
            return;

        var admin = new User
        {
            TenantId = tenant.Id,
            Email = email,
            FirstName = "Admin",
            LastName = tenantName,
            Role = UserRole.Admin
        };
        admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, password);

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}
