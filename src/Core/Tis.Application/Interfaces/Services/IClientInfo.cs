namespace Tis.Application.Interfaces.Services;

// Kim soruşur (ünvan, brauzer): HttpContext-ə toxunmamalı olan servislər üçün, məs. yenilənmə tokeninin haradan
// verildiyini yazmaq.
public interface IClientInfo
{
    string? IpAddress { get; }
    string? UserAgent { get; }
}
