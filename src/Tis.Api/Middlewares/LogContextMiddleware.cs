using Serilog.Context;

namespace Tis.Api.Middlewares;

// Sorğu işlənərkən yazılan hər log sətrinə (servislərin öz sətirlərinə də) kimin soruşduğunu əlavə edir:
// daxil olmuş istifadəçinin id-si (heç kim yoxdursa "-") və müştərinin ünvanı. Autentifikasiyadan sonra işləyir.
public class LogContextMiddleware
{
    private readonly RequestDelegate _next;

    public LogContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("UserId", RequestLogProperties.UserId(context)))
        using (LogContext.PushProperty("ClientIp", RequestLogProperties.ClientIp(context)))
        {
            await _next(context);
        }
    }
}

public static class RequestLogProperties
{
    public static string UserId(HttpContext context) => context.User.FindFirst("sub")?.Value ?? "-";

    public static string ClientIp(HttpContext context) => context.Connection.RemoteIpAddress?.ToString() ?? "-";
}
