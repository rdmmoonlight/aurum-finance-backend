using System.Text.RegularExpressions;
using Aurum.Api.Features.Accounting.Accounts.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.Accounting.Accounts.Validators;

public sealed partial class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    // Mirrors the DB check constraint chk_ref_numeric: ref must be 3-4 digits.
    [GeneratedRegex(@"^\d{3,4}$")]
    private static partial Regex RefPattern();

    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Ref)
            .Must(r => RefPattern().IsMatch(r))
            .WithMessage("Account ref must be 3 or 4 numeric digits (e.g. \"101\", \"5001\").");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Account name cannot be empty.");

        // Only "clearing" is a real role today; null/undefined means a normal account.
        RuleFor(x => x.Role)
            .Must(r => r is null || r == "clearing")
            .WithMessage("Role must be \"clearing\" or omitted.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SortOrder.HasValue);
    }
}
