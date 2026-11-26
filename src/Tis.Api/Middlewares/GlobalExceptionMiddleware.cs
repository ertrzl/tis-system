using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using Tis.Application.Exceptions.Base;
using Tis.Persistence.Extensions;

namespace Tis.Api.Middlewares;

// Servisin atdığı hər BaseException-u HTTP statusuna və kiçik JSON cavaba çevirir: { statusCode, message }.
// Gözlənilməyən hər şey ümumi 500 olur ki, daxili məlumat müştəriyə sızmasın.
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BaseException ex)
        {
            // Sorğunun niyə rədd edildiyi (sorğu sətrində yalnız status kodu olur).
            using var who = Who(context);
            _logger.LogInformation("{Method} {Path} refused ({Status}): {Message}", context.Request.Method, context.Request.Path, ex.StatusCode, ex.Message);
            await WriteErrorAsync(context, ex.StatusCode, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Başqası eyni qeydi oxumaqla saxlamaq arasında dəyişib (və ya silib).
            using var who = Who(context);
            _logger.LogWarning("{Method} {Path}: the data was changed by someone else at the same time", context.Request.Method, context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status409Conflict, Loc.T("server.changedMeanwhile"));
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // İki sorğu "bu artıq varmı?" yoxlamasından eyni anda keçdi; unikal indeks ikincisini tutdu.
            // Müştəri üçün bu, "artıq mövcuddur" deməkdir.
            using var who = Who(context);
            _logger.LogWarning("{Method} {Path}: two requests created the same thing at once (unique index)", context.Request.Method, context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status409Conflict, Loc.T("server.exists"));
        }
        catch (Exception ex)
        {
            using var who = Who(context);
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, Loc.T("server.unexpected"));
        }
    }

    // Bu middleware log sətirlərinə istifadəçini əlavə edən LogContextMiddleware-dən əvvəl işləyir (o, autentifikasiyadan sonra gəlir),
    // ona görə burada yazılan sətirlər istifadəçini və ünvanı özləri əlavə edir.
    private static IDisposable Who(HttpContext context)
    {
        var user = LogContext.PushProperty("UserId", RequestLogProperties.UserId(context));
        var address = LogContext.PushProperty("ClientIp", RequestLogProperties.ClientIp(context));
        return new Both(user, address);
    }

    private sealed class Both : IDisposable
    {
        private readonly IDisposable _first;
        private readonly IDisposable _second;

        public Both(IDisposable first, IDisposable second)
        {
            _first = first;
            _second = second;
        }

        public void Dispose()
        {
            _second.Dispose();
            _first.Dispose();
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { statusCode, message });
    }
}
