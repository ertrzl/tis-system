using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tis.Application.Interfaces.Repositories.Generic;
using Tis.Application.Interfaces.Services;
using Tis.Persistence.Contexts;
using Tis.Persistence.Implementations.Repositories.Generic;
using Tis.Persistence.Implementations.Services;

namespace Tis.Persistence.Extensions;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TisDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // IRepository<Faculty>, IRepository<Department>... hamısı bir ümumi sinifdən qurulur.
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IAuthService, AuthService>();

        // Dövri təmizlik (vaxtı bitmiş tokenlər).
        services.AddHostedService<MaintenanceBackgroundService>();

        // Verilənlər bazası hazırdırsa 200, cavab vermirsə 503 qaytaran /health üçün.
        services.AddHealthChecks().AddDbContextCheck<TisDbContext>();

        return services;
    }
}
