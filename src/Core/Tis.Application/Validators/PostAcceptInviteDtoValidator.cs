using FluentValidation;
using Tis.Application.DTOs.Auth;

namespace Tis.Application.Validators;

public class PostAcceptInviteDtoValidator : AbstractValidator<PostAcceptInviteDto>
{
    public PostAcceptInviteDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(_ => Loc.T("validation.tokenRequired"))
            .MaximumLength(100).WithMessage(_ => Loc.T("validation.tooLong", 100));

        RuleFor(x => x.Password).MustBeStrongPassword();
    }
}
