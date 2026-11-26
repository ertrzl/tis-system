using FluentValidation;
using Tis.Application.DTOs.Auth;

namespace Tis.Application.Validators;

public class PostRefreshDtoValidator : AbstractValidator<PostRefreshDto>
{
    public PostRefreshDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(_ => Loc.T("validation.refreshTokenRequired"))
            .MaximumLength(200).WithMessage(_ => Loc.T("validation.tooLong", 200));
    }
}
