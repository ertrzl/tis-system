using Tis.Application.Exceptions.Base;

namespace Tis.Application.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message) : base(message, 401) { }
}
