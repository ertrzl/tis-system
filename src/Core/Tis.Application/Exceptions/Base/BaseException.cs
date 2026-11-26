namespace Tis.Application.Exceptions.Base;

// Servis qatında atılan bütün "gözlənilən" xətaların əsası. StatusCode-u GlobalExceptionMiddleware HTTP cavabına çevirir.
// Mesaj həmişə Loc.T("açar") ilə verilməlidir ki, istifadəçinin dilində olsun.
public abstract class BaseException : Exception
{
    public int StatusCode { get; }

    protected BaseException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
