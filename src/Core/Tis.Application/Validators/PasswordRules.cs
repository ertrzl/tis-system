using FluentValidation;

namespace Tis.Application.Validators;

public static class PasswordRules
{
    // Parol qaydası: ən azı 8 simvol, böyük hərf, kiçik hərf və rəqəm. Bütün "parol təyin et" yerlərində eynidir.
    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(this IRuleBuilder<T, string> rule) => rule
        .NotEmpty().WithMessage(_ => Loc.T("validation.passwordRequired"))
        .MaximumLength(128).WithMessage(_ => Loc.T("validation.tooLong", 128))
        .Must(IsStrong).WithMessage(_ => Loc.T("validation.passwordWeak"));

    private static bool IsStrong(string password) =>
        password.Length >= 8
        && password.Any(char.IsUpper)
        && password.Any(char.IsLower)
        && password.Any(char.IsDigit);
}
