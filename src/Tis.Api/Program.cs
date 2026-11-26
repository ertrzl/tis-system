using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Tis.Api.Middlewares;
using Tis.Infrastructure.Extensions;
using Tis.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);

// React (Vite) inkişaf serverinə icazə ver
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Dil: Accept-Language başlığından (az, en, ru). Əsas dil: az.
var languages = new[] { "az", "en", "ru" }.Select(c => new CultureInfo(c)).ToList();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("az"),
    SupportedCultures = languages,
    SupportedUICultures = languages
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();   // Docker-də React build-i wwwroot-dan verilir
app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapFallbackToFile("index.html");

await app.MigrateDatabaseIfRequestedAsync();
await app.SeedInitialDataAsync();

app.Run();
