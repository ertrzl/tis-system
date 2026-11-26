using Tis.Application.Exceptions.Base;

namespace Tis.Application.Exceptions;

// Məs: həddindən çox uğursuz giriş cəhdindən sonra hesab müvəqqəti bağlanır.
public class TooManyRequestsException : BaseException
{
    public TooManyRequestsException(string message) : base(message, 429) { }
}
