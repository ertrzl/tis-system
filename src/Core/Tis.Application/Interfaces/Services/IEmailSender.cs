namespace Tis.Application.Interfaces.Services;

// E-poçt göndərir. Infrastructure-də həyata keçirilir: SMTP hesabı qurulubsa onunla göndərilir ("Email:Smtp"),
// qurulmayıbsa Development-də konsola yazılır, digər mühitlərdə xəbərdarlıqla atılır.
public interface IEmailSender
{
    Task SendAsync(EmailMessage message);
}
