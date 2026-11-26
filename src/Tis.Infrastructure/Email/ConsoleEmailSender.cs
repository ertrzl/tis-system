using Microsoft.Extensions.Logging;
using Tis.Application.Interfaces.Services;

namespace Tis.Infrastructure.Email;

// SMTP hesabı olmayanda Development-də istifadə olunur: e-poçt əvəzinə konsola yazılır (bağlantı da daxil),
// beləliklə "parolu təyin et" və "parolu unutdum" axınını poçt hesabı olmadan sınamaq (və göstərmək) olur.
//
// Mesaj birbaşa konsola yazılır, logger-dən KEÇMİR: logger gündəlik log faylını da yazır, iki həftə diskdə qalan faylda
// parol bağlantısı olması oxuyan hər kəs üçün hesaba giriş yoludur. Loga yalnız "mesaj göstərildi" sətri düşür.
public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage message)
    {
        Console.WriteLine($"""

            E-poçt GÖNDƏRİLMƏDİ: SMTP hesabı qurulmayıb (Email:Smtp:Host) və bu Development-dir, ona görə burada göstərilir.
            Kimə: {message.To}
            Mövzu: {message.Subject}

            {message.TextBody}

            """);

        _logger.LogInformation("E-mail to {To} was shown on the console instead of being sent (no SMTP account, Development). Subject: {Subject}",
            message.To, message.Subject);

        return Task.CompletedTask;
    }
}
