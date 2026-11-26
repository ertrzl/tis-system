using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tis.Persistence.Contexts;

namespace Tis.Persistence.Extensions;

public static class DatabaseMigrationExtensions
{
    // "Database:MigrateOnStart" açıqdırsa (Docker), cədvəllər ilk sorğudan əvvəl yaradılır/yenilənir.
    // Standart olaraq söndürülüb: lokalda "dotnet ef database update" işlədilir.
    public static async Task MigrateDatabaseIfRequestedAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Database:MigrateOnStart"))
            return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TisDbContext>();

        // SQL Server konteynerinin hazır olması bir az çəkə bilər: bir neçə dəfə yenidən cəhd edirik.
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                return;
            }
            catch when (attempt < 10)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
