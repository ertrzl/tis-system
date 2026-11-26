namespace Tis.Api.Middlewares;

// Heç nəyə başa gəlməyən brauzer tərəfi qoruması: məzmun tipini təxmin etmə, başqa saytda çərçivəyə alma,
// səhifə ünvanını sızdırmayan referrer və səhifənin yalnız həqiqətən istifadə etdiyini yükləməsinə icazə verən
// content security policy (yalnız öz fayllarımız).
//
// Development-də CSP verilmir: ASP.NET-in SPA proxy yükləmə səhifəsi inline skript işlədir və bloklanardı.
public class SecurityHeadersMiddleware
{
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self' data:; " +
        "connect-src 'self'; " +
        "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";

    private readonly RequestDelegate _next;
    private readonly bool _isDevelopment;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _isDevelopment = environment.IsDevelopment();
    }

    public Task InvokeAsync(HttpContext context)
    {
        // OnStarting, indi yox: xəta middleware-i cavabı (başlıqlar da daxil) yazmazdan əvvəl təmizləyir.
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            if (!_isDevelopment)
                headers["Content-Security-Policy"] = ContentSecurityPolicy;

            // API-nin cavabı bir nəfər haqqındadır (hesabı, qiymətləri): heç bir brauzer və ya proxy surət saxlamasın.
            if (context.Request.Path.StartsWithSegments("/api"))
                headers.CacheControl = "no-store";

            return Task.CompletedTask;
        });

        return _next(context);
    }
}
