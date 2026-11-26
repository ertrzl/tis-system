using Tis.Application.Exceptions.Base;

namespace Tis.Application.Exceptions;

// Məs: bu tələbə nömrəsi və ya e-poçt artıq mövcuddur.
public class ConflictException : BaseException
{
    public ConflictException(string message) : base(message, 409) { }
}
