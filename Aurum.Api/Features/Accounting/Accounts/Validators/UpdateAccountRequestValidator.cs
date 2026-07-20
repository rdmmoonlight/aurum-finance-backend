using System.Text.RegularExpressions;
using Aurum.Api.Features.Accounting.Accounts.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.Accounting.Accounts.Validators;

public sealed partial class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    [GeneratedRegex(@"^\d{3,4}$")]
    private static partial Regex RefPattern();

    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.Ref.Value!)
            .Must(r => RefPattern().IsMatch(r))
            .WithMessage("Account ref must be 3 or 4 numeric digits (e.g. \"101\", \"5001\").")
            .When(x => x.Ref.IsSet);

        RuleFor(x => x.Name.Value!)
            .NotEmpty()
            .WithMessage("Account name cannot be empty.")
            .When(x => x.Name.IsSet);

        RuleFor(x => x.Role.Value)
            .Must(r => r is null || r == "clearing")
            .WithMessage("Role must be \"clearing\" or null.")
            .When(x => x.Role.IsSet);

        RuleFor(x => x.SortOrder.Value)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SortOrder.IsSet);
    }
}
