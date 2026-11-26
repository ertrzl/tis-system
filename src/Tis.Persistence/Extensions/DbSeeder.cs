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
    // İstəyə görə "Seed:DemoStudentNumber" + "Seed:DemoStudentEmail" verilsə, test üçün bir tələbə də yaradılır
    // (parolu adminlə eynidir).
    public static async Task SeedInitialDataAsync(this WebApplication app)
    {
        var password = app.Configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
            return;

        var code = app.Configuration["Seed:TenantCode"] ?? "DEMO";
        var tenantName = app.Configuration["Seed:TenantName"] ?? "Demo Universiteti";
        var adminEmail = (app.Configuration["Seed:AdminEmail"] ?? "admin@demo.az").Trim().ToLowerInvariant();

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TisDbContext>();

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Code == code);
        if (tenant is null)
        {
            tenant = new Tenant { Code = code, Name = tenantName };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        await AddUserIfMissingAsync(db, tenant.Id, adminEmail, null, "Admin", tenantName, UserRole.Admin, password);

        var studentNumber = app.Configuration["Seed:DemoStudentNumber"];
        var studentEmail = app.Configuration["Seed:DemoStudentEmail"];
        if (!string.IsNullOrWhiteSpace(studentNumber) && !string.IsNullOrWhiteSpace(studentEmail))
        {
            await AddUserIfMissingAsync(db, tenant.Id, studentEmail.Trim().ToLowerInvariant(),
                studentNumber.Trim().ToUpperInvariant(), "Test", "Tələbə", UserRole.Student, password);
        }
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
