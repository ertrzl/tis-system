namespace Tis.Application.Exceptions;

// Servis qatında atılır, Api-dəki middleware onları uyğun HTTP statusuna çevirir.
// Mesaj həmişə Loc.T("açar") ilə verilir ki, istifadəçinin dilində olsun.

public class NotFoundException(string message) : Exception(message);      // 404

public class UnauthorizedException(string message) : Exception(message);  // 401

public class ForbiddenException(string message) : Exception(message);     // 403
