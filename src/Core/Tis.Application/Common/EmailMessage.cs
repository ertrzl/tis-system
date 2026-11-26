namespace Tis.Application.Common;

// Bir nəfərə göndərilən bir e-poçt. Mətn versiyası həmişə olur, HTML versiyası isə istəyə bağlıdır.
public record EmailMessage(string To, string Subject, string TextBody, string? HtmlBody = null);
