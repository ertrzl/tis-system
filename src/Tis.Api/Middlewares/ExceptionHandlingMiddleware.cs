using Tis.Application.Exceptions;

namespace Tis.Api.Middlewares;

// Servislərdən atılan xətaları uyğun HTTP statusuna və JSON cavaba çevirir: { "message": "..." }.
// Gözlənilməyən xətalarda detal göstərilmir (təhlükəsizlik), yalnız loga yazılır.
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, message) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, ex.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, Loc.T("common.serverError"))
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Gözlənilməyən xəta");

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new { message });
        }
    }
}
