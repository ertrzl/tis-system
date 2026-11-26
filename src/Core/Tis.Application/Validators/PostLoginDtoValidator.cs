using FluentValidation;
using Tis.Application.DTOs.Auth;

namespace Tis.Application.Validators;

// Hər DTO üçün bir doğrulayıcı. Mətnlər Loc.T ilə Locales/*.json-dan gəlir (üç dildə).
public class PostLoginDtoValidator : AbstractValidator<PostLoginDto>
{
    public PostLoginDtoValidator()
    {
        RuleFor(x => x.TenantCode)
            .NotEmpty().WithMessage(_ => Loc.T("validation.tenantCodeRequired"))
            .MaximumLength(20).WithMessage(_ => Loc.T("validation.tooLong", 20));

        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage(_ => Loc.T("validation.identifierRequired"))
            .MaximumLength(256).WithMessage(_ => Loc.T("validation.tooLong", 256));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_ => Loc.T("validation.passwordRequired"))
            .MaximumLength(128).WithMessage(_ => Loc.T("validation.tooLong", 128));
    }
}
