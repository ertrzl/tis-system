using Tis.Application.Exceptions.Base;

namespace Tis.Application.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message) : base(message, 403) { }
}
