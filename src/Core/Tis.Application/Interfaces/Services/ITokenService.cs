using Tis.Domain.Entities;

namespace Tis.Application.Interfaces.Services;

public interface ITokenService
{
    // İstifadəçi üçün imzalanmış qısa ömürlü JWT yaradır və bitmə vaxtını qaytarır.
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user);

    // Təsadüfi yenilənmə tokeni yaradır: Token müştəriyə verilir, Hash bazada saxlanılır.
    (string Token, string Hash, DateTime ExpiresAt) CreateRefreshToken();

    // Birdəfəlik təsadüfi token (məs: dəvət bağlantısı üçün): Token e-poçta yazılır, Hash bazada saxlanılır.
    (string Token, string Hash) CreateOneTimeToken();

    // Müştərinin göndərdiyi tokenin (yenilənmə və ya dəvət) bazadakı hash-ini hesablayır.
    string HashToken(string token);
}
