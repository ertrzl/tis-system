using FluentValidation;
using Tis.Application.DTOs.Users;

namespace Tis.Application.Validators;

public class UserQueryValidator : AbstractValidator<UserQuery>
{
    public UserQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage(_ => Loc.T("validation.pageInvalid"));
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage(_ => Loc.T("validation.pageSizeInvalid", 100));
        RuleFor(x => x.Search).MaximumLength(100).WithMessage(_ => Loc.T("validation.tooLong", 100));
    }
}
