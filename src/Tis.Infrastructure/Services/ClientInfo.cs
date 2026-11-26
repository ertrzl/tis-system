using Microsoft.AspNetCore.Http;
using Tis.Application.Interfaces.Services;

namespace Tis.Infrastructure.Services;

// Hazırkı sorğunu göndərən müştərinin ünvanı və brauzeri.
public class ClientInfo(IHttpContextAccessor accessor) : IClientInfo
{
    public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent
    {
        get
        {
            var value = accessor.HttpContext?.Request.Headers.UserAgent.ToString();
            // Bazada yer tutmasın deyə uzun başlıq kəsilir.
            return string.IsNullOrEmpty(value) ? null : value.Length > 300 ? value[..300] : value;
        }
    }
}
