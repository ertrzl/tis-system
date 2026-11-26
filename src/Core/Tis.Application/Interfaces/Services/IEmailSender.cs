namespace Tis.Application.Interfaces.Services;

// E-poçt göndərir. Infrastructure-də həyata keçirilir: SMTP hesabı qurulubsa onunla göndərilir ("Email:Smtp"),
// qurulmayıbsa Development-də konsola yazılır, digər mühitlərdə xəbərdarlıqla atılır.
public interface IEmailSender
{
    // Mesaj həqiqətən istifadəçiyə (və ya Development-də konsola) çatır? SMTP qurulmayan Production-da false:
    // belə halda çağıran "e-poçt göndərildi" deməməlidir.
    bool CanDeliver { get; }

    Task SendAsync(EmailMessage message);
}
