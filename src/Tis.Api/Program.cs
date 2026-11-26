using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.OpenApi;
using Serilog;
using Tis.Api.Extensions;
using Tis.Api.Filters;
using Tis.Api.Middlewares;
using Tis.Application.Extensions;
using Tis.Infrastructure.Extensions;
using Tis.Persistence.Extensions;

// Logun ilk sətirləri: konfiqurasiya oxunmamışdan əvvəl yazılır, ona görə uğursuz başlanğıc (səhv ayar, məşğul port)
// da iz qoyur. Aşağıdakı UseSerilog onu konfiqurasiya olunmuş logger ilə əvəz edir.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, shared: true)
    .CreateBootstrapLogger();

// Başqa heç nəyin tutmadığı şeylər: bütün prosesin çökməsi və heç kimin gözləmədiyi arxa plan tapşırığının xətası.
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception: the application is stopping");
    Log.CloseAndFlush();
};
TaskScheduler.UnobservedTaskException += (_, e) =>
{
    Log.Error(e.Exception, "A background task failed and nobody was waiting for it");
    e.SetObserved();
};

var builder = WebApplication.CreateBuilder(args);

// Loglama: nəyin, hara və nə qədər saxlanılması appsettings.json-dakı "Serilog" bölməsindən gəlir
// (konsol və logs/ qovluğunda hər gün üçün bir fayl). Servislər və middleware-lər yalnız ILogger<T> istəyir.
builder.Host.UseSerilog((context, services, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    // Sorğudan kənar yazılan sətirlər (başlanğıc, arxa plan işi) üçün standart dəyərlər: həqiqiləri LogContextMiddleware qoyur.
    .Enrich.WithProperty("UserId", "-")
    .Enrich.WithProperty("ClientIp", "-"));

// "Server: Kestrel" başlığı yoxdur: yad adama yalnız hansı proqramdakı boşluğu axtaracağını deyir.
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TIS API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Yalnız JWT-ni yapışdırın ('Bearer ' yazmayın) — Swagger onu özü əlavə edir."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services
    .AddApplicationServices()
    .AddPersistenceServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiRateLimiting(builder.Configuration);

// React saytının ünvanları (inkişafda Vite). Docker-də sayt və API eyni ünvandan verildiyi üçün CORS lazım olmur.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// "Database:MigrateOnStart" açıqdırsa (Docker), cədvəllər ilk sorğudan əvvəl yaradılır/yenilənir.
await app.MigrateDatabaseIfRequestedAsync();
await app.SeedInitialDataAsync();

// Bu işə salınmanın bir neçə faktı, log nə haqqında olduğunu göstərməklə başlasın.
Log.Information("Starting in {Environment}. Public address set: {HasPublicAddress}. SMTP configured: {HasSmtp}",
    app.Environment.EnvironmentName,
    !string.IsNullOrWhiteSpace(builder.Configuration["App:PublicBaseUrl"]),
    !string.IsNullOrWhiteSpace(builder.Configuration["Email:Smtp:Host"]));

// Hər sorğu üçün bir sətir (metod, ünvan, status, vaxt). Səhifə, skript və şəkillər üçün ayrıca sətir yazmağa dəyməz,
// onlar (və health check) yalnız Debug səviyyəsində yazılır.
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (context, elapsed, exception) =>
    {
        var path = context.Request.Path;
        var isFile = Path.HasExtension(path) || path.StartsWithSegments("/health");
        if (exception != null || context.Response.StatusCode >= 500)
            return Serilog.Events.LogEventLevel.Error;
        if (isFile)
            return Serilog.Events.LogEventLevel.Debug;
        // İki saniyədən çox çəkən sorğu işləsə belə baxmağa dəyər.
        return context.Response.StatusCode >= 400 || elapsed > 2000 ? Serilog.Events.LogEventLevel.Warning : Serilog.Events.LogEventLevel.Information;
    };

    // Kim, haradan, nə ilə: sətir sorğudan sonra, istifadəçi məlum olanda yazılır.
    options.EnrichDiagnosticContext = (diagnostic, context) =>
    {
        diagnostic.Set("UserId", RequestLogProperties.UserId(context));
        diagnostic.Set("ClientIp", RequestLogProperties.ClientIp(context));
        diagnostic.Set("UserAgent", context.Request.Headers.UserAgent.ToString());
    };
});

// Sorğunun dili (Accept-Language, səhifə göndərir) mesajların dilini təyin edir. Dillər Locales/<kod>.json faylı olan
// dillərdir (bax: Loc), yəni fayl əlavə etmək dil əlavə edir.
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(Loc.DefaultLanguage)
    .AddSupportedCultures(Loc.Languages.ToArray())
    .AddSupportedUICultures(Loc.Languages.ToArray()));

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    // Səhifə və skriptlər tez-tez dəyişir. Açıq başlıq olmasa brauzer özü qərar verir və köhnə faylla işləyə bilər.
    // "no-cache": həmişə serverdən soruş, fayl dəyişməyibsə 304 (gövdəsiz) cavab gəlir.
    OnPrepareResponse = context =>
    {
        var extension = Path.GetExtension(context.File.Name);
        if (extension is ".html")
            context.Context.Response.Headers.CacheControl = "no-cache";
    }
});

app.UseAuthentication();
app.UseAuthorization();

// Autentifikasiyadan sonra, istifadəçi məlum olsun: bundan sonra hər log sətri istifadəçini və ünvanı daşıyır.
app.UseMiddleware<LogContextMiddleware>();

// Autentifikasiyadan sonra, ümumi limit daxil olmuş istifadəçiyə görə sayılsın.
app.UseRateLimiter();

// Hər controller action-ı daxil olmuş şəxs tələb edir, yalnız [AllowAnonymous] (giriş) yazılıbsa istisna.
// Sonradan əlavə olunan controller [Authorize] unudulsa, açıq yox, bağlı qalır.
app.MapControllers().RequireAuthorization();

// Monitorinq və load balancer üçün: verilənlər bazası cavab verirsə 200 "Healthy", əks halda 503.
app.MapHealthChecks("/health").AllowAnonymous();

// React-in yönləndirmələri (məs: /login) üçün index.html qaytarılır.
app.MapFallbackToFile("index.html");

app.Run();
