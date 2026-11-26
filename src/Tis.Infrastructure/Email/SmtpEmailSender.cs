using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Tis.Application.Interfaces.Services;

namespace Tis.Infrastructure.Email;

// Ayarlardakı SMTP hesabı ("Email:Smtp") ilə göndərir. İstənilən provayderlə işləyir: Gmail (tətbiq parolu), Outlook,
// Brevo, şirkət poçt serveri və ya Mailtrap/Papercut kimi test qutusu.
public class SmtpEmailSender : IEmailSender
{
    public bool CanDeliver => true;

    private const int TimeoutMilliseconds = 15_000;

    private readonly EmailOptions _options;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(EmailMessage message)
    {
        var smtp = _options.Smtp;
        var fromAddress = string.IsNullOrWhiteSpace(_options.FromAddress) ? smtp.UserName : _options.FromAddress;

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_options.FromName, fromAddress));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { TextBody = message.TextBody, HtmlBody = message.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient { Timeout = TimeoutMilliseconds };
        await client.ConnectAsync(smtp.Host, smtp.Port, ConnectionSecurity(smtp));

        if (!string.IsNullOrEmpty(smtp.UserName))
            await client.AuthenticateAsync(smtp.UserName, smtp.Password);

        await client.SendAsync(mime);
        await client.DisconnectAsync(quit: true);
    }

    private static SecureSocketOptions ConnectionSecurity(SmtpOptions smtp)
    {
        if (!smtp.UseSsl)
            return SecureSocketOptions.None;

        return smtp.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
    }
}
