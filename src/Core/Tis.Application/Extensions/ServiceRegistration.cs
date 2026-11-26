using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tis.Application.Extensions;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper 15-dən sonra lisenziya açarı istəyir (pulsuz Community açarı kifayətdir): "AutoMapper:LicenseKey".
        // Açar olmasa da proqram eyni işləyir, yalnız hər başlanğıcda xəbərdarlıq yazılır.
        services.AddAutoMapper(
            (serviceProvider, cfg) => cfg.LicenseKey = serviceProvider.GetService<IConfiguration>()?["AutoMapper:LicenseKey"],
            Assembly.GetExecutingAssembly());

        // Validators qovluğundakı bütün doğrulayıcılar avtomatik tapılır. Onları hər sorğuda ValidationFilter işlədir (Api).
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
