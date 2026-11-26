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
    // İlk məlumatlar. Parollar heç vaxt koda yazılmır: appsettings.Development.json və ya mühit dəyişənindən gəlir.
    //  - "Seed:SuperAdminPassword" təyin olunubsa platforma universiteti və SuperAdmin yaradılır (yeni universitetləri o açır).
    //  - "Seed:AdminPassword" təyin olunubsa nümunə universitet (DEMO) və onun admini yaradılır; istəyə görə
    //    "Seed:DemoStudentNumber" + "Seed:DemoStudentEmail" ilə bir test tələbəsi də (parolu adminlə eyni).
    // Parol təyin olunmayıbsa ilgili hissə atlanır.
    public static async Task SeedInitialDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TisDbContext>();

        await SeedSuperAdminAsync(app.Configuration, db);
        await SeedDemoTenantAsync(app.Configuration, db);
    }

    private static async Task SeedSuperAdminAsync(IConfiguration configuration, TisDbContext db)
    {
        var password = configuration["Seed:SuperAdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
            return;

        var email = (configuration["Seed:SuperAdminEmail"] ?? "superadmin@tis.az").Trim().ToLowerInvariant();

        var platform = await GetOrCreateTenantAsync(db, Tenant.PlatformCode, "Platform");
        await AddUserIfMissingAsync(db, platform.Id, email, null, "Super", "Admin", UserRole.SuperAdmin, password);
    }

    private static async Task SeedDemoTenantAsync(IConfiguration configuration, TisDbContext db)
    {
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
            return;

        var code = (configuration["Seed:TenantCode"] ?? "DEMO").Trim().ToUpperInvariant();
        var tenantName = configuration["Seed:TenantName"] ?? "Demo Universiteti";
        var adminEmail = (configuration["Seed:AdminEmail"] ?? "admin@demo.az").Trim().ToLowerInvariant();

        var tenant = await GetOrCreateTenantAsync(db, code, tenantName);
        await AddUserIfMissingAsync(db, tenant.Id, adminEmail, null, "Admin", tenantName, UserRole.Admin, password);

        var studentNumber = configuration["Seed:DemoStudentNumber"];
        var studentEmail = configuration["Seed:DemoStudentEmail"];
        if (!string.IsNullOrWhiteSpace(studentNumber) && !string.IsNullOrWhiteSpace(studentEmail))
        {
            await AddUserIfMissingAsync(db, tenant.Id, studentEmail.Trim().ToLowerInvariant(),
                studentNumber.Trim().ToUpperInvariant(), "Test", "Tələbə", UserRole.Student, password);
        }
    }

    private static async Task<Tenant> GetOrCreateTenantAsync(TisDbContext db, string code, string name)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Code == code);
        if (tenant is not null)
            return tenant;

        tenant = new Tenant { Code = code, Name = name };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        return tenant;
    }

    private static async Task AddUserIfMissingAsync(TisDbContext db, int tenantId, string email, string? studentNumber,
        string firstName, string lastName, UserRole role, string password)
    {
        var exists = await db.Users.IgnoreQueryFilters().AnyAsync(u => u.TenantId == tenantId && u.Email == email);
        if (exists)
            return;

        var user = new User
        {
            TenantId = tenantId,
            Email = email,
            StudentNumber = studentNumber,
            FirstName = firstName,
            LastName = lastName,
            Role = role
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
