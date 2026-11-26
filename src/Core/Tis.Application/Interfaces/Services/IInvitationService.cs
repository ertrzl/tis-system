using Tis.Domain.Entities;

namespace Tis.Application.Interfaces.Services;

public interface IInvitationService
{
    // İstifadəçi üçün dəvət yaradır və e-poçtla göndərir. Dəvət bazaya YAZILIR (SaveChanges çağırılır).
    // E-poçt göndərilə bildisə true qaytarır; göndərmə uğursuz olsa xəta atmır (dəvət yenə də yaradılıb, sonra təkrar göndərilə bilər).
    Task<bool> CreateAndSendAsync(User user, string tenantName, CancellationToken ct = default);
}
