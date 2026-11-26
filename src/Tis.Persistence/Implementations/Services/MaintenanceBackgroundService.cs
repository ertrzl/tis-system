using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tis.Persistence.Contexts;

namespace Tis.Persistence.Implementations.Services;

// Arxa planda dövri təmizlik: vaxtı bitmiş və ya ləğv olunmuş yenilənmə tokenləri bir həftə sonra silinir ki, cədvəl
// ildən-ilə şişməsin. Bir həftə saxlanılır ki, təhlükəsizlik araşdırması üçün yaxın keçmişin izi qalsın.
public class MaintenanceBackgroundService : BackgroundService
{
    private static readonly TimeSpan FirstRunDelay = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan KeepFor = TimeSpan.FromDays(7);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MaintenanceBackgroundService> _logger;

    public MaintenanceBackgroundService(IServiceScopeFactory scopeFactory, ILogger<MaintenanceBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(FirstRunDelay, stoppingToken);

            using var timer = new PeriodicTimer(Interval);
            do
            {
                await CleanUpAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Proqram dayanır: normaldır.
        }
    }

    private async Task CleanUpAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TisDbContext>();

            var limit = DateTime.UtcNow - KeepFor;
            var deleted = await db.RefreshTokens
                .Where(t => t.ExpiresAt < limit || (t.RevokedAt != null && t.RevokedAt < limit))
                .ExecuteDeleteAsync(ct);

            if (deleted > 0)
                _logger.LogInformation("Maintenance: {Count} old refresh tokens deleted", deleted);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Təmizlik uğursuz olsa proqram dayanmasın: növbəti dövrdə yenidən cəhd olunur.
            _logger.LogError(ex, "Maintenance run failed");
        }
    }
}
