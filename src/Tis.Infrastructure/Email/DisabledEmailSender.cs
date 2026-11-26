using Microsoft.Extensions.Logging;
using Tis.Application.Interfaces.Services;

namespace Tis.Infrastructure.Email;

// Development-dən kənarda SMTP hesabı qurulmayıbsa istifadə olunur. Mesaj atılır və yalnız xəbərdarlıq yazılır:
// mətnində parol bağlantısı var və log faylında bağlantı olması, loga çıxışı olan hər kəsə hesabları ələ keçirməyə imkan verərdi.
public class DisabledEmailSender : IEmailSender
{
    // Heç nə göndərilmir: çağıran bunu bilməlidir ki, istifadəçiyə "e-poçt göndərildi" deməsin.
    public bool CanDeliver => false;

    private readonly ILogger<DisabledEmailSender> _logger;

    public DisabledEmailSender(ILogger<DisabledEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage message)
    {
        _logger.LogWarning("An e-mail was not sent because no SMTP account is configured (Email:Smtp:Host). Subject: {Subject}", message.Subject);
        return Task.CompletedTask;
    }
}
