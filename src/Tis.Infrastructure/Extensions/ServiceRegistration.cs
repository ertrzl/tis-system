using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tis.Application.Interfaces.Services;
using Tis.Infrastructure.Email;
using Tis.Infrastructure.Services;

namespace Tis.Infrastructure.Extensions;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Açar çox qısadırsa tətbiq başlayanda aydın xəta ilə dayanır (dotnet ef əmrləri bundan təsirlənmir).
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => o.SecretKey.Length >= 32, "Jwt:SecretKey ən azı 32 simvol olmalıdır.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IClientInfo, ClientInfo>();
        services.AddSingleton<ITokenService, TokenService>();

        // E-poçt ayarlardakı SMTP hesabı ilə göndərilir. Hesab yoxdursa Development-də konsola yazılır,
        // digər mühitlərdə xəbərdarlıqla atılır.
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddSingleton<IEmailSender>(provider =>
        {
            if (provider.GetRequiredService<IOptions<EmailOptions>>().Value.Smtp.IsConfigured)
                return ActivatorUtilities.CreateInstance<SmtpEmailSender>(provider);

            return provider.GetRequiredService<IHostEnvironment>().IsDevelopment()
                ? ActivatorUtilities.CreateInstance<ConsoleEmailSender>(provider)
                : ActivatorUtilities.CreateInstance<DisabledEmailSender>(provider);
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Token yoxlama ayarları JwtOptions hazır olandan sonra qurulur.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearer.MapInboundClaims = false;   // "sub", "role" adları dəyişdirilmədən qalsın
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    NameClaimType = "sub",
                    RoleClaimType = TokenService.RoleClaim,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
