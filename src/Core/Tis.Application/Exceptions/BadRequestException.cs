using Tis.Application.Exceptions.Base;

namespace Tis.Application.Exceptions;

public class BadRequestException : BaseException
{
    public BadRequestException(string message) : base(message, 400) { }
}
