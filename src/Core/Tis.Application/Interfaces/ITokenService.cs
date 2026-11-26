using Tis.Domain.Entities;

namespace Tis.Application.Interfaces;

public interface ITokenService
{
    // İstifadəçi üçün imzalanmış JWT yaradır və bitmə vaxtını qaytarır.
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
