using FluentValidation;
using Tis.Application.DTOs.Tenants;
using Tis.Domain.Entities;

namespace Tis.Application.Validators;

public class PostTenantDtoValidator : AbstractValidator<PostTenantDto>
{
    public PostTenantDtoValidator()
    {
        // Kod girişdə universitet seçimi üçün yazılır: 2-20 simvol, yalnız hərf, rəqəm və tire.
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(_ => Loc.T("validation.tenantCodeRequired"))
            .Matches("^[A-Za-z0-9-]{2,20}$").WithMessage(_ => Loc.T("validation.codeFormat"))
            .Must(code => !string.Equals(code, Tenant.PlatformCode, StringComparison.OrdinalIgnoreCase))
            .WithMessage(_ => Loc.T("tenant.codeReserved"));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_ => Loc.T("validation.nameRequired"))
            .MaximumLength(200).WithMessage(_ => Loc.T("validation.tooLong", 200));

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage(_ => Loc.T("validation.emailRequired"))
            .EmailAddress().WithMessage(_ => Loc.T("validation.emailInvalid"))
            .MaximumLength(256).WithMessage(_ => Loc.T("validation.tooLong", 256));

        RuleFor(x => x.AdminFirstName)
            .NotEmpty().WithMessage(_ => Loc.T("validation.firstNameRequired"))
            .MaximumLength(100).WithMessage(_ => Loc.T("validation.tooLong", 100));

        RuleFor(x => x.AdminLastName)
            .NotEmpty().WithMessage(_ => Loc.T("validation.lastNameRequired"))
            .MaximumLength(100).WithMessage(_ => Loc.T("validation.tooLong", 100));
    }
}
