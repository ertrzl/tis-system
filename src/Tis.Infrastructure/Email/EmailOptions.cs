namespace Tis.Infrastructure.Email;

// Ayarlardakı "Email" bölməsi. SMTP serveri olmadan heç nə həqiqətən göndərilmir (bax: ServiceRegistration).
public class EmailOptions
{
    public const string SectionName = "Email";

    // İnsanların gördüyü göndərən. Gmail və əksər provayderlər hesabın öz ünvanını tələb edir, ona görə
    // boş olarsa SMTP istifadəçi adı istifadə olunur.
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "TİS";

    public SmtpOptions Smtp { get; set; } = new();
}

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Bağlantını şifrələ (587 portunda STARTTLS, 465-də ilk baytdan TLS). Yalnız lokal test serveri üçün söndür.
    public bool UseSsl { get; set; } = true;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
