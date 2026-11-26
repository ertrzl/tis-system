using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tis.Application.Interfaces.Services;
using Tis.Domain.Entities;
using Tis.Persistence.Contexts;

namespace Tis.Persistence.Implementations.Services;

public class InvitationService(
    TisDbContext db,
    ITokenService tokenService,
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<InvitationService> logger) : IInvitationService
{
    public async Task<bool> CreateAndSendAsync(User user, string tenantName, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var days = configuration.GetValue("Auth:InviteDays", 7);
        var (token, hash) = tokenService.CreateOneTimeToken();

        // Yeni dəvət gedəndə köhnə istifadə olunmamış dəvətlər keçərsiz olur: yalnız ən son bağlantı işləyir.
        await db.UserInvitations
            .Where(i => i.UserId == user.Id && i.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.UsedAt, now), ct);

        db.UserInvitations.Add(new UserInvitation
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            TokenHash = hash,
            ExpiresAt = now.AddDays(days)
        });
        await db.SaveChangesAsync(ct);

        // E-poçtdakı bağlantı React saytının ünvanına yönəlir ("App:PublicBaseUrl").
        var baseUrl = (configuration["App:PublicBaseUrl"] ?? string.Empty).TrimEnd('/');
        var link = $"{baseUrl}/accept-invite?token={token}";

        try
        {
            await emailSender.SendAsync(new EmailMessage(
                user.Email,
                Loc.T("email.invite.subject", tenantName),
                Loc.T("email.invite.body", user.FirstName, tenantName, days, link)));

            return emailSender.CanDeliver;
        }
        catch (Exception ex)
        {
            // Dəvət yaradılıb: e-poçt uğursuz olsa belə əməliyyat uğurludur, bağlantı sonra təkrar göndərilə bilər.
            logger.LogError(ex, "Invitation e-mail to user {UserId} could not be sent", user.Id);
            return false;
        }
    }
}
